using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;

namespace Engine_003_showTabelData
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public ILayer pGlobalFeatureLayer;

        private void axTOCControl1_OnMouseDown(object sender, ESRI.ArcGIS.Controls.ITOCControlEvents_OnMouseDownEvent e)
        {
            if (axMapControl1.LayerCount > 0)
            {
            esriTOCControlItem pItem = new esriTOCControlItem();
            pGlobalFeatureLayer = new FeatureLayerClass();
            IBasicMap pBasicMap = new MapClass();
            object pOther = new object();

            object pIndex = new object();
            axTOCControl1.HitTest(e.x, e.y,ref pItem, ref pBasicMap,ref pGlobalFeatureLayer,ref pOther,ref  pIndex);
            }
            if (e.button == 2)
            {
            
                context.Show(axTOCControl1, e.x, e.y);
            }

        }

      

       

        private void 打开属性表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
          FormTable Ft = new FormTable(pGlobalFeatureLayer as IFeatureLayer);
            Ft.Show();
        }


       
    }
}
