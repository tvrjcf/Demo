using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Demo_WebService
{
    public partial class Form1 : Form
    {
        private string strErr;
        WebServiceReference.WebServiceSoapClient WS = new WebServiceReference.WebServiceSoapClient();

        SRMWebService.WSForWmsKdIClient client = new SRMWebService.WSForWmsKdIClient();
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_GetCurrentIP_Click(object sender, EventArgs e)
        {
            this.label1.Text = WS.GetCurrentIP(ref strErr);
            if (!string.IsNullOrEmpty(strErr))
            {
                MessageBox.Show(strErr);
            }
        }

        private void btn_EncryptMD5_Click(object sender, EventArgs e)
        {
            string str = "123456";
            str = WS.EncryptMD5(str, ref strErr);
            if (!string.IsNullOrEmpty(strErr))
            {
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show(str);
            }
        }

        private void btn_StrToMd5_Click(object sender, EventArgs e)
        {
            string str = "123456";
            str = WS.StrToMd5(str);
            if (!string.IsNullOrEmpty(strErr))
            {
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show(str);
            }
        }

        private void btn_SRM_Click(object sender, EventArgs e)
        {
            SRMWebService.requWmsReqList list = new SRMWebService.requWmsReqList();
            list.requWmsHead = new SRMWebService.requWmsHead() { code= "ASN201701120668" };
            list.requList = new SRMWebService.requWmsBody[] {
                new SRMWebService.requWmsBody (){
                    itemCode ="3099999997",
                    qualifiedQty =0,
                    quantity=100,
                    RTVquantity=0,
                    scrapquantity=0,
                    SRMLineNo=1,
                    status="S1",
                    unit="PC",
                    qualifiedQtySpecified =true,
                    quantitySpecified =true,
                    RTVquantitySpecified =true,
                    scrapquantitySpecified =true,
                    SRMLineNoSpecified =true
                    
                }
            };

            var xml = client.getWms(list);

        }
    }
}
