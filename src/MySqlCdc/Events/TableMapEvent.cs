using System.Collections;
using MySqlCdc.Providers.MySql;

namespace MySqlCdc.Events
{
    /// <summary>
    /// The event has table defition for row events.
    /// <a href="https://mariadb.com/kb/en/library/table_map_event/">See more</a>
    /// </summary>
    public class TableMapEvent : BinlogEvent
    {
        public long TableId { get; }
        public string DatabaseName { get; }
        public string TableName { get; }
        public byte[] ColumnTypes { get; }
        public int[] ColumnMetadata { get; }
        public BitArray NullBitmap { get; }
        public TableMetadata TableMetadata { get; }

        public TableMapEvent(
            EventHeader header,
            long tableId,
            string databaseName,
            string tableName,
            byte[] columnTypes,
            int[] columnMetadata,
            BitArray nullBitmap,
            TableMetadata tableMetadata) : base(header)
        {
            TableId = tableId;
            DatabaseName = databaseName;
            TableName = tableName;
            ColumnTypes = columnTypes;
            ColumnMetadata = columnMetadata;
            NullBitmap = nullBitmap;
            TableMetadata = tableMetadata;
        }
    }
}
