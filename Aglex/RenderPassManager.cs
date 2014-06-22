using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aglex
{
    public class RenderPassManagerException : Exception
    {
        public RenderPassManagerException() : base() { }
        public RenderPassManagerException(string message) : base(message) { }
        public RenderPassManagerException(string message, Exception innerException) : base(message, innerException) { }
        public RenderPassManagerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class RenderPassManager
    {
        int maxRenderPasses;
        RenderPass[] renderPasses;

        public int CurrentPass { get; private set; }

        public RenderPassManager(int maxPasses)
        {
            this.maxRenderPasses = maxPasses;
            this.renderPasses = new RenderPass[this.maxRenderPasses];
        }

        public void SetRenderPass(int number, RenderPass info)
        {
            if (number >= this.maxRenderPasses) throw new RenderPassManagerException(string.Format("Cannot set render pass {0}; max render pass is {1}", number, this.maxRenderPasses));
            this.renderPasses[number] = info;
        }

        public void Perform(Action renderAction)
        {
            for (int i = 0; i < this.maxRenderPasses; i++)
            {
                if (renderPasses[i] == null) continue;
                renderPasses[CurrentPass = i].Perform(renderAction);
            }
        }
    }
}
