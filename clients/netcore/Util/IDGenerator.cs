using System;
using System.Text;
using System.Security.Cryptography;

namespace Tributech.Dsk.Api.Clients.Util
{
    public static class IDGenerator
    {
        /// <summary>
        /// Generates a deterministic DataStreamID based on a hash of the input values.
        /// </summary>
        /// <param name="agentID">Uniquely identifies a single agent</param>
        /// <param name="dataSourceType">The type of data source. E.g. "AzureOpcUaPublisher"</param>
        /// <param name="dataSourceIdentifyer">Uniquely identifies the data source. E.g.: "opc.tcp://hostname.com:4840/demo"</param>
        /// <param name="sensorName">Uniquely identifies the sensor/data stream within the data source. E.g.: "ns=2;i=12" or "machine_temperature"</param>
        /// <returns></returns>
        public static Guid GenerateDataStreamID(string agentID, string dataSourceType, string dataSourceIdentifyer, string sensorName)
        {
            var sourceString = String.Concat(agentID, dataSourceType, dataSourceIdentifyer, sensorName);
            byte[] stringbytes = Encoding.UTF8.GetBytes(sourceString);
            byte[] hashedBytes = new SHA512CryptoServiceProvider()
                .ComputeHash(stringbytes);
            Array.Resize(ref hashedBytes, 16);
            return new Guid(hashedBytes);
        }
    }
}