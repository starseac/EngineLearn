using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace Engine_003_showTabelData
{
    public partial class FormTable : Form
    {
        IFeatureLayer pFeatureLayer;
        public FormTable(IFeatureLayer pFeatureLayer)
        {
            InitializeComponent();
            this.pFeatureLayer=pFeatureLayer;
            Itable2Dtable();
        }

        public void Itable2Dtable()
        {
            IFields pFields;
            pFields = pFeatureLayer.FeatureClass.Fields;
            dataGridView1.ColumnCount = pFields.FieldCount;
            for (int i = 0; i < pFields.FieldCount; i++)
            {
                string fldName = pFields.get_Field(i).Name;
                dataGridView1.Columns[i].Name = fldName;
                dataGridView1.Columns[i].ValueType = System.Type.GetType(ParseFieldType(pFields.get_Field(i).Type));
            }
            IFeatureCursor pFeatureCursor;

            pFeatureCursor = pFeatureLayer.FeatureClass.Search(null, false);
            IFeature pFeature;
            pFeature = pFeatureCursor.NextFeature();
            while (pFeature != null)
            {
                string[] fldValue = new string[pFields.FieldCount];
                for (int i = 0; i < pFields.FieldCount; i++)
                {
                    string fldName;
                    fldName = pFields.get_Field(i).Name;
                    if (fldName == pFeatureLayer.FeatureClass.ShapeFieldName)
                    {
                        fldValue[i] = Convert.ToString(pFeature.Shape.GeometryType);
                    }
                    else
                        fldValue[i] = Convert.ToString(pFeature.get_Value(i));
                }
                dataGridView1.Rows.Add(fldValue);
                pFeature = pFeatureCursor.NextFeature();
            }
        }

        private string ParseFieldType(esriFieldType esriFieldType)
        {
            return "string";
        }

    }
}
