using System.Linq;
using System.Text;
using MySql.Cdc.Constants;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Commands
{
    /// <summary>
    /// Client handshake response to the server initial handshake packet.
    /// <see cref="https://mariadb.com/kb/en/library/connection/#handshake-response-packet"/>
    /// </summary>
    public class AuthenticateCommand : ICommand
    {
        public int ClientCapabilities { get; private set; }
        public int ClientCollation { get; private set; }
        public int MaxPacketSize { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Scramble { get; private set; }
        public string Database { get; private set; }

        public AuthenticateCommand(ConnectionOptions options, int clientCollation, string scramble, int maxPacketSize = 0)
        {
            ClientCollation = clientCollation;
            MaxPacketSize = maxPacketSize;
            Scramble = scramble;
            Username = options.Username;
            Password = options.Password;
            Database = options.Database;

            ClientCapabilities = (int)CapabilityFlags.LONG_FLAG
                | (int)CapabilityFlags.PROTOCOL_41
                | (int)CapabilityFlags.SECURE_CONNECTION;

            if (Database != null)
                ClientCapabilities |= (int)CapabilityFlags.CONNECT_WITH_DB;
        }

        public byte[] CreatePacket(byte sequenceNumber)
        {
            var writer = new PacketWriter(sequenceNumber);
            writer.WriteInt(ClientCapabilities, 4);
            writer.WriteInt(MaxPacketSize, 4);
            writer.WriteInt(ClientCollation, 1);

            // Fill reserved bytes 
            for (int i = 0; i < 23; i++)
                writer.WriteByte(0);

            writer.WriteNullTerminatedString(Username);
            byte[] sha1 = GetMySqlNativePasswordHash(Password, Scramble);
            writer.WriteByte((byte)sha1.Length);
            writer.WriteByteArray(sha1);

            if (Database != null)
                writer.WriteNullTerminatedString(Database);

            return writer.CreatePacket();
        }

        public static byte[] GetMySqlNativePasswordHash(string password, string scramble)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var passwordHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
            var concatHash = Encoding.UTF8.GetBytes(scramble).Concat(sha1.ComputeHash(passwordHash)).ToArray();
            return Xor(passwordHash, sha1.ComputeHash(concatHash));
        }

        private static byte[] Xor(byte[] array1, byte[] array2)
        {
            byte[] result = new byte[array1.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = (byte)(array1[i] ^ array2[i]);
            return result;
        }
    }

    public class MySqlNativePasswordPluginCommand : ICommand
    {
        public string Password { get; private set; }
        public string Scramble { get; private set; }

        public MySqlNativePasswordPluginCommand(string password, string scramble)
        {
            Password = password;
            Scramble = scramble;
        }

        public byte[] CreatePacket(byte sequenceNumber)
        {
            var writer = new PacketWriter(sequenceNumber);
            writer.WriteByteArray(AuthenticateCommand.GetMySqlNativePasswordHash(Password, Scramble));
            return writer.CreatePacket();
        }
    }
}
