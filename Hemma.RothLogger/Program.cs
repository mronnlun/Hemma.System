﻿using Hemma.Entities;
using Hemma.Entities.v2;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Hemma.RothLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new WebClient();
            client.Headers.Add("Accept-Language", "*");
            client.Headers.Add("Content-Type", "text/xml");
            client.Headers.Add("User-Agent", "SpiderControl/1.0 (iniNet-Solutions GmbH)");

            client.Headers.Add("Cache-Control", "no-cache");
            client.Headers.Add("Pragma", "no-cache");
            client.Headers.Add("Accept", "text/html, image/gif, image/jpeg, *; q = .2, */*; q=.2");

            var body = File.ReadAllText("RequestBody.xml");
            //body = "<body><item_list><i><n>G0.RaumTemp</n></i><i><n>G1.RaumTemp</n></i><i><n>G2.RaumTemp</n></i><i><n>G3.RaumTemp</n></i><i><n>G4.RaumTemp</n></i><i><n>G5.RaumTemp</n></i><i><n>G6.RaumTemp</n></i><i><n>G7.RaumTemp</n></i><i><n>G8.RaumTemp</n></i><i><n>G9.RaumTemp</n></i></item_list></body>";
            var raw = client.UploadData("http://192.168.0.104/cgi-bin/ILRReadValues.cgi", "POST",
                System.Text.Encoding.ASCII.GetBytes(body));

            string text = System.Text.Encoding.ASCII.GetString(raw);

            var xml = XDocument.Parse(text);
            var rothData = new RothData();
            rothData.Vardagsrum = GetRoomValue(xml, "G0.RaumTemp");
            rothData.VardagsrumSetting = GetRoomValue(xml, "G0.SollTemp");
            rothData.Hallen = GetRoomValue(xml, "G1.RaumTemp");
            rothData.HallenSetting = GetRoomValue(xml, "G1.SollTemp");
            rothData.Lekrum = GetRoomValue(xml, "G2.RaumTemp");
            rothData.LekrumSetting = GetRoomValue(xml, "G2.SollTemp");
            rothData.MammasPappas = GetRoomValue(xml, "G3.RaumTemp");
            rothData.MammasPappasSetting = GetRoomValue(xml, "G3.SollTemp");
            rothData.Viggo = GetRoomValue(xml, "G4.RaumTemp");
            rothData.ViggoSetting = GetRoomValue(xml, "G4.SollTemp");
            rothData.Felix = GetRoomValue(xml, "G5.RaumTemp");
            rothData.FelixSetting = GetRoomValue(xml, "G5.SollTemp");
            rothData.Arbetsrum = GetRoomValue(xml, "G6.RaumTemp");
            rothData.ArbetsrumSetting = GetRoomValue(xml, "G6.SollTemp");
            var now = DateTime.Now;
            rothData.Timestamp = now.Ticks;
            rothData.Datestamp = now;

            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase("Roth");

            var datas = database.GetCollection<RothData>("LoggedData");
            datas.InsertOne(rothData);

            //var oldDataColl = database.GetCollection<Hemma.Entities.v1.RothData>("Data");
            //var oldDatas = oldDataColl.AsQueryable().ToList();
            //var newDatas = new List<RothData>();
            //foreach (var oldItem in oldDatas)
            //{
            //    var newdata = new RothData();
            //    var stamp = new DateTime(oldItem.Timestamp);
            //    newdata.Datestamp = stamp;
            //    newdata.Arbetsrum = Convert.ToDouble(oldItem.Arbetsrum);
            //    newdata.ArbetsrumSetting = Convert.ToDouble(oldItem.ArbetsrumSetting);
            //    newdata.Felix = Convert.ToDouble(oldItem.Felix);
            //    newdata.FelixSetting = Convert.ToDouble(oldItem.FelixSetting);
            //    newdata.Hallen = Convert.ToDouble(oldItem.Hallen);
            //    newdata.HallenSetting = Convert.ToDouble(oldItem.HallenSetting);
            //    newdata.Lekrum = Convert.ToDouble(oldItem.Lekrum);
            //    newdata.LekrumSetting = Convert.ToDouble(oldItem.LekrumSetting);
            //    newdata.MammasPappas = Convert.ToDouble(oldItem.MammasPappas);
            //    newdata.MammasPappasSetting = Convert.ToDouble(oldItem.MammasPappasSetting);
            //    newdata.Timestamp = oldItem.Timestamp;
            //    newdata.Vardagsrum = Convert.ToDouble(oldItem.Vardagsrum);
            //    newdata.VardagsrumSetting = Convert.ToDouble(oldItem.VardagsrumSetting);
            //    newdata.Viggo = Convert.ToDouble(oldItem.Viggo);
            //    newdata.ViggoSetting = Convert.ToDouble(oldItem.ViggoSetting);


            //    newDatas.Add(newdata);
            //}
            //datas.InsertMany(newDatas);

        }

        private static double GetRoomValue(XDocument xml, string roomId)
        {
            var textvalue = (from item in xml.Descendants("i")
                             where item.Descendants("n").First().Value.Equals(roomId)
                             select item.Descendants("v").First().Value).Single();

            int number;
            if (!int.TryParse(textvalue, out number))
                return 0;

            var dec = number / 100D;

            return Math.Round(dec, 1);
        }
    }
}
