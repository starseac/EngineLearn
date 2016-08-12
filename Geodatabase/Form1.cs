using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;
using System.IO;

namespace Geodatabase
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.txt_server.Text = "127.0.0.1";
            this.txt_instance.Text = "sde:oracle10g:orcl";
            // this.txt_database.Text = "";
            this.txt_user.Text = "sde";
            this.txt_password.Text = "sa";

            this.txt_featuredatasetname.Text = "TEST";

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IAoInitialize pao = new AoInitializeClass();
            pao.Initialize(esriLicenseProductCode.esriLicenseProductCodeStandard);
        }

        IWorkspace workspace;
        IFeatureWorkspace featureworkspace;
        IFeatureDataset featuredataset;

        private void button1_Click(object sender, EventArgs e)
        {

            EngineDatabase engineBase = new EngineDatabase();
            workspace = engineBase.getSDEWorkspace(this.txt_server.Text, this.txt_instance.Text, this.txt_user.Text, this.txt_password.Text, workspace);
            if (workspace != null)
            {
                MessageBox.Show("sde链接成功!");
            }
            else
            {
                MessageBox.Show("sde链接失败!");
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            EngineDatabase engine = new EngineDatabase();
            engine.createSDE("Oracle","ORCL", "SYS", "SA", "TEST", "SA", "GEO_TEST", @"C:\\Server101.ecp");
        }

        private void button3_Click(object sender, EventArgs e)
        {

            EngineDatabase engineBase = new EngineDatabase();
            engineBase.createFeatureDataset(workspace, featureworkspace, featuredataset, 2360, this.txt_user.Text, this.txt_featuredatasetname.Text);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            EngineDatabase engineBase = new EngineDatabase();
            engineBase.createGDBFile("c:\\", "Empty2.gdb");
            engineBase.exportSDE2GDB(workspace, featureworkspace, featuredataset, "c:\\Empty2.gdb", 2360, "sde");

        }

        private void button4_Click(object sender, EventArgs e)
        {
            featureworkspace = workspace as IFeatureWorkspace;
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "shp file(*,shp)|*.shp";
            fileDialog.Title = "打开矢量数据";
            fileDialog.Multiselect = false;
            string fileName = "";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = fileDialog.FileName;


                EngineDatabase engineBase = new EngineDatabase();
                engineBase.importSHP2SDE(featureworkspace, featuredataset, "TEST", fileName);
                MessageBox.Show("导入成功");
            }

            }

        private void btn_fromgdb_Click(object sender, EventArgs e)
        {

            //string WsName = filePath();
            string WsName = FolderPath();
            if (WsName != "")
            {
                IWorkspaceFactory pWsFt;

                pWsFt = new FileGDBWorkspaceFactoryClass();
                IWorkspace pWs = pWsFt.OpenFromFile(WsName, 0);
                IEnumDataset pEDataset = pWs.get_Datasets(esriDatasetType.esriDTAny);
                IDataset pDataset = pEDataset.Next();
                while (pDataset != null)
                {
                    EngineDatabase engine = new EngineDatabase();

                    //如果是数据集
                    if (pDataset.Type == esriDatasetType.esriDTFeatureDataset)
                    {
                        return;
                        //创建 featureDataset

                        engine.createFeatureDataset(workspace, featureworkspace, featuredataset, 2360, "sde", pDataset.Name);
                        featureworkspace = workspace as IFeatureWorkspace;


                        IEnumDataset pESubDataset = pDataset.Subsets;
                        IDataset pSubDataset = pESubDataset.Next();
                        while (pSubDataset != null)
                        {
                            //导入featuredateset下的 featureclass
                            IFeatureWorkspace shpfwp = pWs as IFeatureWorkspace;
                            IFeatureClass shpfc = shpfwp.OpenFeatureClass(pSubDataset.Name);


                            //导入featureclass
                            IFeatureClass sdeFeatureClass = null;
                            IFeatureClassDescription featureClassDescription = new FeatureClassDescriptionClass();
                            IObjectClassDescription objectClassDescription = featureClassDescription as IObjectClassDescription;
                            IFields fields = shpfc.Fields;
                            IFieldChecker fieldChecker = new FieldCheckerClass();
                            IEnumFieldError enumFieldError = null;
                            IFields validateFields = null;
                            fieldChecker.ValidateWorkspace = featureworkspace as IWorkspace;
                            fieldChecker.Validate(fields, out enumFieldError, out validateFields);
                            featuredataset = featureworkspace.OpenFeatureDataset(pDataset.Name);

                            try
                            {
                                sdeFeatureClass = featureworkspace.OpenFeatureClass(pSubDataset.Name);
                            }
                            catch
                            {

                            }

                            //在sde数据库中创建feature class
                            if (sdeFeatureClass == null || sdeFeatureClass.FeatureDataset.Name != pDataset.Name)
                            {
                                sdeFeatureClass = featuredataset.CreateFeatureClass(pSubDataset.Name, validateFields, objectClassDescription.InstanceCLSID,
                                    objectClassDescription.ClassExtensionCLSID, shpfc.FeatureType,
                                    shpfc.ShapeFieldName, "");
                            }

                            //导入featureclass 的数据
                            //开始编辑sde featureclass
                            IFeatureCursor featureCursor = shpfc.Search(null, true);
                            IFeature feature = featureCursor.NextFeature();
                            IFeatureCursor sdeFeatureCursor = sdeFeatureClass.Insert(true);
                            IFeatureBuffer sdeFeatureBuffer;
                            //导入数据
                            while (feature != null)
                            {
                                sdeFeatureBuffer = sdeFeatureClass.CreateFeatureBuffer();
                                IField shpField = new FieldClass();
                                IFields shpFields = feature.Fields;
                                for (int i = 0; i < shpFields.FieldCount; i++)
                                {
                                    shpField = shpFields.get_Field(i);
                                    string fieldname = "";

                                    if (shpField.Name == "FID")
                                    {
                                        fieldname = "OBJECTID";
                                    }
                                    else if (shpField.Name == "Shape")
                                    {
                                        fieldname = "SHAPE";
                                    }
                                    else if (shpField.Name == "SHAPE_Leng")
                                    {
                                        fieldname = "SHAPE.LEN";
                                    }
                                    else if (shpField.Name == "SHAPE_Area")
                                    {
                                        fieldname = "SHAPE.AREA";
                                    }
                                    else
                                    {
                                        fieldname = shpField.Name;
                                    }
                                    int index = sdeFeatureBuffer.Fields.FindField(fieldname);
                                    if (index != -1 && index != 0 && fieldname != "SHAPE.LEN" && fieldname != "SHAPE.AREA")
                                    {
                                        sdeFeatureBuffer.set_Value(index, feature.get_Value(i));
                                    }
                                }
                                sdeFeatureCursor.InsertFeature(sdeFeatureBuffer);
                                sdeFeatureCursor.Flush();
                                feature = featureCursor.NextFeature();
                            }


                            MessageBox.Show("" + pSubDataset.Name + "数据导入完成");
                            pSubDataset = pESubDataset.Next();
                        }
                    }
                    else if (pDataset.Type == esriDatasetType.esriDTTable)
                    {
                        // 创建表          

                        ITable table = pDataset as ITable;
                        IFields fields = table.Fields;
                        ITable sdeTable = engine.CreateTable(workspace, pDataset.Name, pDataset.BrowseName, fields);
                        ///导入数据                      

                        ICursor cursor = table.Search(null, true);
                        IRow row = cursor.NextRow();
                        ICursor sdeCursor = sdeTable.Insert(true);
                        IRowBuffer sdeBuffer;
                        //导入数据
                        while (row != null)
                        {
                            sdeBuffer = sdeTable.CreateRowBuffer();
                            IField shpField = new FieldClass();
                            IFields shpFields = row.Fields;
                            for (int i = 0; i < shpFields.FieldCount; i++)
                            {
                                shpField = shpFields.get_Field(i);
                                string fieldname = shpField.Name;
                                int index = sdeBuffer.Fields.FindField(fieldname);
                                if (index != -1 && index != 0)
                                {
                                    sdeBuffer.set_Value(index, row.get_Value(i));
                                }
                            }
                            sdeCursor.InsertRow(sdeBuffer);
                            sdeCursor.Flush();
                            row = cursor.NextRow();

                          
                            // FeatureClassBox.Items.Add("table_" + pDataset.Name);
                        }
                        MessageBox.Show("" + pDataset.Name + "数据导入完成");
                        pDataset = pEDataset.Next();
                    }
                }

            }
        }


        private string filePath()
        {

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "文件|*.mdb;*.shp;*.gdb";
            fileDialog.Title = "打开矢量数据";
            fileDialog.Multiselect = false;
            string fileName = "";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = fileDialog.FileName;
            }
            return fileName;
        }

        private string FolderPath()
        {

            FolderBrowserDialog fileDialog = new FolderBrowserDialog();

            string fileName = "";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = fileDialog.SelectedPath;
            }
            return fileName;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //string WsName = filePath();
            string WsName = FolderPath();
            if (WsName != "")
            {
                //IWorkspaceFactory pWsFt;
                ////FileInfo file = new FileInfo(WsName);
                ////if (file.Extension.ToUpper().ToString() == ".GDB")
                ////{
                //pWsFt = new FileGDBWorkspaceFactoryClass();
                ////}
                ////else
                ////{
                ////     pWsFt = new AccessWorkspaceFactoryClass();
                ////}
                //IWorkspace pWs = pWsFt.OpenFromFile(WsName, 0);
                IWorkspace pWs = workspace;

                IEnumDataset pEDataset = pWs.get_Datasets(esriDatasetType.esriDTAny);
                IDataset pDataset = pEDataset.Next();
                while (pDataset != null)
                {
                    if (pDataset.Type == esriDatasetType.esriDTFeatureClass)
                    {
                        FeatureClassBox.Items.Add("feature_"+pDataset.Name);
                    }
                    //如果是数据集
                    else if (pDataset.Type == esriDatasetType.esriDTFeatureDataset)
                    {
                        IEnumDataset pESubDataset = pDataset.Subsets;
                        IDataset pSubDataset = pESubDataset.Next();
                        while (pSubDataset != null)
                        {
                            FeatureClassBox.Items.Add(pDataset.Name + "_" + pSubDataset.Name);
                            pSubDataset = pESubDataset.Next();
                        }
                    }
                    else if (pDataset.Type == esriDatasetType.esriDTTable)
                    {
                        FeatureClassBox.Items.Add("table_" + pDataset.Name);
                    }
                    pDataset = pEDataset.Next();
                }
            }
            FeatureClassBox.Text = FeatureClassBox.Items[0].ToString();

        }

        private void button7_Click(object sender, EventArgs e)
        {
            EngineDatabase engine = new EngineDatabase();
            engine.deleteFeatureDataset(workspace, "TEST");
            MessageBox.Show("要素集删除成功");

        }

        private void button9_Click(object sender, EventArgs e)
        {
            EngineDatabase engine = new EngineDatabase();
            engine.deleteFeatureClass(workspace, "DJQ");
            MessageBox.Show("要素删除成功");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            EngineDatabase engine = new EngineDatabase();
            engine.deleteTable(workspace, "C");
            MessageBox.Show("表删除成功");
        }



    }
}
