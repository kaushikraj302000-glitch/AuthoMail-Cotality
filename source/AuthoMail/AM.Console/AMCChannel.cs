using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    class AMCChannel : Channel
    {
        public AMCChannel(string channelCode)
            : base(channelCode) { }

        protected override void EnumerateBatches()
        {
            throw new NotImplementedException();
        }

        protected override void DownloadBatches()
        {
            throw new NotImplementedException();
        }

        protected override void ValidateBatches()
        {
            throw new NotImplementedException();
        }

        protected override void AcknowledgeBatches()
        {
            throw new NotImplementedException();
        }
    }
}
