using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Aglex
{
    public class RenderPass
    {
        public EnableCap Enable { get; set; }

        public CullFaceMode CullFace { get; set; }

        public PolygonMode PolygonModeFront { get; set; }
        public PolygonMode PolygonModeBack { get; set; }

        public StencilFunction StencilFunction { get; set; }
        public int StencilReference { get; set; }
        public int StencilMask { get; set; }
        public StencilOp StencilFailOp { get; set; }
        public StencilOp StencilZFailOp { get; set; }
        public StencilOp StencilZPassOp { get; set; }

        public RenderPass()
        {
            Enable |= EnableCap.CullFace;

            /* Culling */
            CullFace = CullFaceMode.Back;

            /* Polygon mode */
            PolygonModeFront = PolygonMode.Fill;
            PolygonModeBack = PolygonMode.Fill;

            /* Stencil */
            StencilFunction = StencilFunction.Always;
            StencilReference = 0;
            StencilMask = -1;
            StencilFailOp = StencilOp.Keep;
            StencilZFailOp = StencilOp.Keep;
            StencilZPassOp = StencilOp.Keep;
        }

        public void Perform(Action renderAction)
        {
            if (renderAction == null) throw new RenderPassManagerException("RenderPass action is null");

            SetStates();

            renderAction();
        }

        private void SetStates()
        {
            /* Culling */
            if (GL.IsEnabled(EnableCap.CullFace) && !Enable.HasFlag(EnableCap.CullFace))
                GL.Disable(EnableCap.CullFace);
            else if (Enable.HasFlag(EnableCap.CullFace))
            {
                if (!GL.IsEnabled(EnableCap.CullFace)) GL.Enable(EnableCap.CullFace);

                CullFaceMode lastCullFace = (CullFaceMode)GL.GetInteger(GetPName.CullFace);
                if (lastCullFace != CullFace) GL.CullFace(CullFace);
            }

            /* Polygon mode */
            int[] lastPolyMode = new int[2];
            GL.GetInteger(GetPName.PolygonMode, lastPolyMode);

            if ((PolygonMode)lastPolyMode[0] != PolygonModeBack) GL.PolygonMode(MaterialFace.Front, PolygonModeFront);
            if ((PolygonMode)lastPolyMode[1] != PolygonModeBack) GL.PolygonMode(MaterialFace.Back, PolygonModeBack);

            /* Stencil */
            if (GL.IsEnabled(EnableCap.StencilTest) && !Enable.HasFlag(EnableCap.StencilTest))
                GL.Disable(EnableCap.StencilTest);
            else if (!Enable.HasFlag(EnableCap.StencilTest))
            {
                if (!GL.IsEnabled(EnableCap.StencilTest)) GL.Enable(EnableCap.StencilTest);

                GL.StencilFunc(StencilFunction, StencilReference, StencilMask);
                GL.StencilOp(StencilFailOp, StencilZFailOp, StencilZPassOp);
            }
        }

        public static RenderPass Default = new RenderPass();
        public static RenderPass RenderBackFace = new RenderPass() { Enable = EnableCap.CullFace, CullFace = CullFaceMode.Front };
    }
}
