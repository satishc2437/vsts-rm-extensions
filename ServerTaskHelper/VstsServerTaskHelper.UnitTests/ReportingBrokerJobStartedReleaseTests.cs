﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VstsServerTaskHelper.UnitTests
{
    /// <summary>
    /// Unit test class for <see cref="ReportingBrokerJobStartedReleaseTests"/> class.
    /// </summary>
    [TestClass]
    public class ReportingBrokerJobStartedReleaseTests
    {
        [TestMethod]
        public async Task ReportsStartedTest()
        {
            // given
            var returnNullRelease = false;
            var releaseStatus = ReleaseStatus.Active;

            // then
            var expectedEventCount = 1;

            // when
            await TestReportJobStarted(releaseStatus, returnNullRelease, expectedEventCount);
        }

        [TestMethod]
        public async Task SkipsNullReleaseTest()
        {
            // given
            var returnNullRelease = true;
            var releaseStatus = ReleaseStatus.Undefined;

            // then
            var expectedEventCount = 0;

            // when
            await TestReportJobStarted(releaseStatus, returnNullRelease, expectedEventCount);
        }

        [TestMethod]
        public async Task SkipsUndefinedReleaseTest()
        {
            // given
            var returnNullRelease = false;
            var releaseStatus = ReleaseStatus.Undefined;

            // then
            var expectedEventCount = 0;

            // when
            await TestReportJobStarted(releaseStatus, returnNullRelease, expectedEventCount);
        }

        private static async Task TestReportJobStarted(ReleaseStatus releaseStatus, bool returnNullRelease, int expectedEventCount)
        {
            // given
            VstsMessage vstsContext = new TestVstsMessage
            {
                VstsHub = HubType.Release,
                VstsUri = new Uri("http://vstsUri"),
                VstsPlanUri = new Uri("http://vstsPlanUri"),
                ReleaseProperties = new VstsReleaseProperties(),
            };

            var mockBuildClient = new MockBuildClient()
            {
                MockBuild = new Build() { Status = BuildStatus.None },
                ReturnNullBuild = false,
            };
            var mockReleaseClient = new MockReleaseClient()
            {
                MockRelease = new Release() { Status = releaseStatus },
                ReturnNullRelease = returnNullRelease,
            };
            var mockTaskClient = new MockTaskClient();
            Assert.AreNotEqual(vstsContext.VstsUri, vstsContext.VstsPlanUri, "need to be different to ensure we can test correct one is used");
            var reportingHelper = new TestableJobStatusReportingHelper(vstsContext, new TraceLogger(), mockTaskClient, mockReleaseClient, mockBuildClient);

            // when
            await reportingHelper.ReportJobStarted(DateTime.UtcNow, "test message", default(CancellationToken));

            // then
            Assert.AreEqual(expectedEventCount, mockTaskClient.EventsReceived.Count);
            if (expectedEventCount != 0)
            {
                var taskEvent = mockTaskClient.EventsReceived[0] as JobStartedEvent;
                Assert.IsNotNull(taskEvent);
            }
        }
    }
}
