using Microsoft.Data.Sqlite;
using ISO11820.App.Models;

namespace ISO11820.App.Data;

/// <summary>
/// SQLite 数据库操作封装
/// 所有 SQL 均使用参数化查询
/// </summary>
public class DbHelper
{
    private readonly string _connStr;

    public DbHelper(string dbPath)
    {
        // 确保数据库文件所在目录存在
        var dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        _connStr = $"Data Source={dbPath}";
    }

    // ================================================================
    // 数据库初始化
    // ================================================================

    /// <summary>
    /// 首次运行时建表并写入初始数据
    /// </summary>
    public void InitializeDatabase()
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();

        // 检查是否已初始化（operators 表是否存在）
        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='operators'";
        var tableCount = (long)checkCmd.ExecuteScalar()!;

        if (tableCount > 0)
            return; // 已初始化

        CreateTables(conn);
        SeedData(conn);
    }

    private void CreateTables(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS ""operators"" (
                ""userid""    TEXT NOT NULL,
                ""username""  TEXT NOT NULL,
                ""pwd""       TEXT NOT NULL,
                ""usertype""  TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ""apparatus"" (
                ""apparatusid""   INTEGER NOT NULL CONSTRAINT ""PK_apparatus"" PRIMARY KEY,
                ""innernumber""   TEXT NOT NULL,
                ""apparatusname"" TEXT NOT NULL,
                ""checkdatef""    date NOT NULL,
                ""checkdatet""    date NOT NULL,
                ""pidport""       TEXT NOT NULL,
                ""powerport""     TEXT NOT NULL,
                ""constpower""    INTEGER NULL
            );

            CREATE TABLE IF NOT EXISTS ""productmaster"" (
                ""productid""   TEXT NOT NULL CONSTRAINT ""PK_productmaster"" PRIMARY KEY,
                ""productname"" TEXT NOT NULL,
                ""specific""    TEXT NOT NULL,
                ""diameter""    REAL NOT NULL,
                ""height""      REAL NOT NULL,
                ""flag""        TEXT NULL
            );

            CREATE TABLE IF NOT EXISTS ""testmaster"" (
                ""productid""        TEXT NOT NULL,
                ""testid""           TEXT NOT NULL,
                ""testdate""         date NOT NULL,
                ""ambtemp""          REAL NOT NULL,
                ""ambhumi""          REAL NOT NULL,
                ""according""        TEXT NOT NULL,
                ""operator""         TEXT NOT NULL,
                ""apparatusid""      TEXT NOT NULL,
                ""apparatusname""    TEXT NOT NULL,
                ""apparatuschkdate"" date NOT NULL,
                ""rptno""            TEXT NOT NULL,
                ""preweight""        REAL NOT NULL,
                ""postweight""       REAL NOT NULL,
                ""lostweight""       REAL NOT NULL,
                ""lostweight_per""   REAL NOT NULL,
                ""totaltesttime""    INTEGER NOT NULL,
                ""constpower""       INTEGER NOT NULL,
                ""phenocode""        TEXT NOT NULL,
                ""flametime""        INTEGER NOT NULL,
                ""flameduration""    INTEGER NOT NULL,
                ""maxtf1""           REAL NOT NULL,
                ""maxtf2""           REAL NOT NULL,
                ""maxts""            REAL NOT NULL,
                ""maxtc""            REAL NOT NULL,
                ""maxtf1_time""      INTEGER NOT NULL,
                ""maxtf2_time""      INTEGER NOT NULL,
                ""maxts_time""       INTEGER NOT NULL,
                ""maxtc_time""       INTEGER NOT NULL,
                ""finaltf1""         REAL NOT NULL,
                ""finaltf2""         REAL NOT NULL,
                ""finalts""          REAL NOT NULL,
                ""finaltc""          REAL NOT NULL,
                ""finaltf1_time""    INTEGER NOT NULL,
                ""finaltf2_time""    INTEGER NOT NULL,
                ""finalts_time""     INTEGER NOT NULL,
                ""finaltc_time""     INTEGER NOT NULL,
                ""deltatf1""         REAL NOT NULL,
                ""deltatf2""         REAL NOT NULL,
                ""deltatf""          REAL NOT NULL,
                ""deltats""          REAL NOT NULL,
                ""deltatc""          REAL NOT NULL,
                ""memo""             TEXT NULL,
                ""flag""             TEXT NULL,
                CONSTRAINT ""PK_testmaster"" PRIMARY KEY (""productid"", ""testid""),
                CONSTRAINT ""FK_testmaster_productmaster"" FOREIGN KEY (""productid"")
                    REFERENCES ""productmaster"" (""productid"")
            );

            CREATE INDEX IF NOT EXISTS ""IX_Testmaster_Testdate""
                ON ""testmaster"" (""testdate"");
            CREATE INDEX IF NOT EXISTS ""IX_Testmaster_Operator""
                ON ""testmaster"" (""operator"");
            CREATE INDEX IF NOT EXISTS ""IX_Testmaster_Testdate_Productid""
                ON ""testmaster"" (""testdate"", ""productid"");

            CREATE TABLE IF NOT EXISTS ""sensors"" (
                ""sensorid""    INTEGER NOT NULL CONSTRAINT ""PK_sensors"" PRIMARY KEY,
                ""sensorname""  TEXT NOT NULL,
                ""dispname""    TEXT NOT NULL,
                ""sensorgroup"" TEXT NOT NULL,
                ""unit""        TEXT NOT NULL,
                ""discription"" TEXT NOT NULL,
                ""flag""        TEXT NOT NULL,
                ""signalzero""  REAL NOT NULL,
                ""signalspan""  REAL NOT NULL,
                ""outputzero""  REAL NOT NULL,
                ""outputspan""  REAL NOT NULL,
                ""outputvalue"" REAL NOT NULL,
                ""inputvalue""  REAL NOT NULL,
                ""signaltype""  INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ""CalibrationRecords"" (
                ""Id""                 TEXT NOT NULL CONSTRAINT ""PK_CalibrationRecords"" PRIMARY KEY,
                ""CalibrationDate""    TEXT NOT NULL,
                ""CalibrationType""    TEXT NOT NULL,
                ""ApparatusId""        INTEGER NOT NULL,
                ""Operator""           TEXT NOT NULL,
                ""TemperatureData""    TEXT NOT NULL,
                ""UniformityResult""   REAL NULL,
                ""MaxDeviation""       REAL NULL,
                ""AverageTemperature"" REAL NULL,
                ""PassedCriteria""     INTEGER NOT NULL,
                ""Remarks""            TEXT NOT NULL,
                ""CreatedAt""          TEXT NOT NULL,
                ""TempA1"" REAL NULL, ""TempA2"" REAL NULL, ""TempA3"" REAL NULL,
                ""TempB1"" REAL NULL, ""TempB2"" REAL NULL, ""TempB3"" REAL NULL,
                ""TempC1"" REAL NULL, ""TempC2"" REAL NULL, ""TempC3"" REAL NULL,
                ""TAvg""        REAL NULL,
                ""TAvgAxis1""   REAL NULL, ""TAvgAxis2"" REAL NULL, ""TAvgAxis3"" REAL NULL,
                ""TAvgLevela""  REAL NULL, ""TAvgLevelb"" REAL NULL, ""TAvgLevelc"" REAL NULL,
                ""TDevAxis1""   REAL NULL, ""TDevAxis2"" REAL NULL, ""TDevAxis3"" REAL NULL,
                ""TDevLevela""  REAL NULL, ""TDevLevelb"" REAL NULL, ""TDevLevelc"" REAL NULL,
                ""TAvgDevAxis"" REAL NULL, ""TAvgDevLevel"" REAL NULL,
                ""CenterTempData"" TEXT NULL,
                ""Memo""           TEXT NULL
            );

            CREATE INDEX IF NOT EXISTS ""IX_CalibrationRecord_Date""
                ON ""CalibrationRecords"" (""CalibrationDate"");
            CREATE INDEX IF NOT EXISTS ""IX_CalibrationRecord_Operator""
                ON ""CalibrationRecords"" (""Operator"");
        ";
        cmd.ExecuteNonQuery();
    }

    private void SeedData(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            INSERT INTO operators (userid, username, pwd, usertype)
            SELECT '1', 'admin', '123456', 'admin'
            WHERE NOT EXISTS (SELECT 1 FROM operators WHERE username = 'admin');

            INSERT INTO operators (userid, username, pwd, usertype)
            SELECT '2', 'experimenter', '123456', 'operator'
            WHERE NOT EXISTS (SELECT 1 FROM operators WHERE username = 'experimenter');

            INSERT INTO apparatus (apparatusid, innernumber, apparatusname, checkdatef, checkdatet, pidport, powerport, constpower)
            SELECT 0, 'FURNACE-01', '一号试验炉', date('now'), date('now', '+1 year'), 'COM9', 'COM9', 2048
            WHERE NOT EXISTS (SELECT 1 FROM apparatus WHERE apparatusid = 0);
        ";
        cmd.ExecuteNonQuery();

        // 插入传感器（17个通道）
        SeedSensors(conn);
    }

    private void SeedSensors(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();
        // 先检查是否已有传感器数据
        cmd.CommandText = "SELECT COUNT(*) FROM sensors";
        var count = (long)cmd.ExecuteScalar()!;
        if (count > 0) return;

        // 5 个主要传感器 + 12 个备用
        var sensors = new (int id, string name, string disp, string group, string unit, string desc)[]
        {
            (0,  "Sensor0",  "炉温1",    "采集", "℃", "炉温1"),
            (1,  "Sensor1",  "炉温2",    "采集", "℃", "炉温2"),
            (2,  "Sensor2",  "表面温度", "采集", "℃", "表面温度"),
            (3,  "Sensor3",  "中心温度", "采集", "℃", "中心温度"),
            (4,  "Sensor4",  "备用通道5",  "采集", "℃", "备用通道5"),
            (5,  "Sensor5",  "备用通道6",  "采集", "℃", "备用通道6"),
            (6,  "Sensor6",  "备用通道7",  "采集", "℃", "备用通道7"),
            (7,  "Sensor7",  "备用通道8",  "采集", "℃", "备用通道8"),
            (8,  "Sensor8",  "备用通道9",  "采集", "℃", "备用通道9"),
            (9,  "Sensor9",  "备用通道10", "采集", "℃", "备用通道10"),
            (10, "Sensor10", "备用通道11", "采集", "℃", "备用通道11"),
            (11, "Sensor11", "备用通道12", "采集", "℃", "备用通道12"),
            (12, "Sensor12", "备用通道13", "采集", "℃", "备用通道13"),
            (13, "Sensor13", "备用通道14", "采集", "℃", "备用通道14"),
            (14, "Sensor14", "备用通道15", "采集", "℃", "备用通道15"),
            (15, "Sensor15", "备用通道16", "采集", "℃", "备用通道16"),
            (16, "Sensor16", "校准温度", "校准", "℃", "校准温度"),
        };

        cmd.CommandText = @"
            INSERT INTO sensors
                (sensorid, sensorname, dispname, sensorgroup, unit, discription,
                 flag, signalzero, signalspan, outputzero, outputspan, outputvalue, inputvalue, signaltype)
            VALUES
                ($id, $name, $disp, $grp, $unit, $desc,
                 '启用', 0, 0, 0, 1000, 0, 0, 4)";

        foreach (var s in sensors)
        {
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("$id", s.id);
            cmd.Parameters.AddWithValue("$name", s.name);
            cmd.Parameters.AddWithValue("$disp", s.disp);
            cmd.Parameters.AddWithValue("$grp", s.group);
            cmd.Parameters.AddWithValue("$unit", s.unit);
            cmd.Parameters.AddWithValue("$desc", s.desc);
            cmd.ExecuteNonQuery();
        }
    }

    // ================================================================
    // 登录验证
    // ================================================================

    /// <summary>
    /// 验证操作员登录
    /// </summary>
    /// <returns>是否登录成功</returns>
    public bool Login(string username, string pwd, out string userid, out string usertype)
    {
        userid = string.Empty;
        usertype = string.Empty;

        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT userid, usertype FROM operators WHERE username=$name AND pwd=$pwd";
        cmd.Parameters.AddWithValue("$name", username);
        cmd.Parameters.AddWithValue("$pwd", pwd);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            userid = reader.GetString(0);
            usertype = reader.GetString(1);
            return true;
        }
        return false;
    }

    // ================================================================
    // 设备信息
    // ================================================================

    /// <summary>
    /// 获取设备信息（全局只有一条，id=0）
    /// </summary>
    public Apparatus? GetApparatus()
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM apparatus WHERE apparatusid = 0";

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Apparatus
            {
                ApparatusId = reader.GetInt32(0),
                InnerNumber = reader.GetString(1),
                ApparatusName = reader.GetString(2),
                CheckDateF = DateTime.Parse(reader.GetString(3)),
                CheckDateT = DateTime.Parse(reader.GetString(4)),
                PidPort = reader.GetString(5),
                PowerPort = reader.GetString(6),
                ConstPower = reader.IsDBNull(7) ? null : reader.GetInt32(7)
            };
        }
        return null;
    }

    /// <summary>
    /// 更新设备信息
    /// </summary>
    public void UpdateApparatus(Apparatus apparatus)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE apparatus SET
                innernumber=$inner, apparatusname=$name,
                checkdatef=$chkf, checkdatet=$chkt,
                pidport=$pid, powerport=$power, constpower=$cp
            WHERE apparatusid=$id";
        cmd.Parameters.AddWithValue("$inner", apparatus.InnerNumber);
        cmd.Parameters.AddWithValue("$name", apparatus.ApparatusName);
        cmd.Parameters.AddWithValue("$chkf", apparatus.CheckDateF.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$chkt", apparatus.CheckDateT.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$pid", apparatus.PidPort);
        cmd.Parameters.AddWithValue("$power", apparatus.PowerPort);
        cmd.Parameters.AddWithValue("$cp", apparatus.ConstPower ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$id", apparatus.ApparatusId);
        cmd.ExecuteNonQuery();
    }

    // ================================================================
    // 样品信息
    // ================================================================

    /// <summary>
    /// 插入或更新样品信息
    /// </summary>
    public void UpsertProduct(ProductMaster product)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO productmaster (productid, productname, specific, diameter, height, flag)
            VALUES ($pid, $name, $spec, $dia, $h, $flag)
            ON CONFLICT(productid) DO UPDATE SET
                productname = excluded.productname,
                specific    = excluded.specific,
                diameter    = excluded.diameter,
                height      = excluded.height,
                flag        = excluded.flag";
        cmd.Parameters.AddWithValue("$pid", product.ProductId);
        cmd.Parameters.AddWithValue("$name", product.ProductName);
        cmd.Parameters.AddWithValue("$spec", product.Specific);
        cmd.Parameters.AddWithValue("$dia", product.Diameter);
        cmd.Parameters.AddWithValue("$h", product.Height);
        cmd.Parameters.AddWithValue("$flag", product.Flag ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 获取所有样品列表
    /// </summary>
    public List<ProductMaster> GetAllProducts()
    {
        var list = new List<ProductMaster>();
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM productmaster ORDER BY productid";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new ProductMaster
            {
                ProductId = reader.GetString(0),
                ProductName = reader.GetString(1),
                Specific = reader.GetString(2),
                Diameter = reader.GetDouble(3),
                Height = reader.GetDouble(4),
                Flag = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }
        return list;
    }

    /// <summary>
    /// 按编号查找样品
    /// </summary>
    public ProductMaster? GetProduct(string productId)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM productmaster WHERE productid=$pid";
        cmd.Parameters.AddWithValue("$pid", productId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new ProductMaster
            {
                ProductId = reader.GetString(0),
                ProductName = reader.GetString(1),
                Specific = reader.GetString(2),
                Diameter = reader.GetDouble(3),
                Height = reader.GetDouble(4),
                Flag = reader.IsDBNull(5) ? null : reader.GetString(5)
            };
        }
        return null;
    }

    // ================================================================
    // 传感器
    // ================================================================

    /// <summary>
    /// 获取所有传感器配置
    /// </summary>
    public List<Sensor> GetSensors()
    {
        var list = new List<Sensor>();
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM sensors ORDER BY sensorid";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(ReadSensor(reader));
        }
        return list;
    }

    /// <summary>
    /// 获取单个传感器
    /// </summary>
    public Sensor? GetSensor(int sensorId)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM sensors WHERE sensorid=$id";
        cmd.Parameters.AddWithValue("$id", sensorId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return ReadSensor(reader);
        return null;
    }

    /// <summary>
    /// 更新传感器当前值（供 DaqWorker 每帧调用）
    /// </summary>
    public void UpdateSensorValue(int sensorId, double outputValue, double inputValue)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE sensors SET outputvalue=$ov, inputvalue=$iv WHERE sensorid=$id";
        cmd.Parameters.AddWithValue("$ov", outputValue);
        cmd.Parameters.AddWithValue("$iv", inputValue);
        cmd.Parameters.AddWithValue("$id", sensorId);
        cmd.ExecuteNonQuery();
    }

    private static Sensor ReadSensor(SqliteDataReader reader)
    {
        return new Sensor
        {
            SensorId = reader.GetInt32(0),
            SensorName = reader.GetString(1),
            DispName = reader.GetString(2),
            SensorGroup = reader.GetString(3),
            Unit = reader.GetString(4),
            Discription = reader.GetString(5),
            Flag = reader.GetString(6),
            SignalZero = reader.GetDouble(7),
            SignalSpan = reader.GetDouble(8),
            OutputZero = reader.GetDouble(9),
            OutputSpan = reader.GetDouble(10),
            OutputValue = reader.GetDouble(11),
            InputValue = reader.GetDouble(12),
            SignalType = reader.GetInt32(13)
        };
    }

    // ================================================================
    // 试验记录 (testmaster)
    // ================================================================

    /// <summary>
    /// 新建试验记录（大部分统计字段初始化为 0）
    /// </summary>
    public void InsertTest(TestMaster test)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO testmaster
                (productid, testid, testdate, ambtemp, ambhumi,
                 according, operator, apparatusid, apparatusname, apparatuschkdate, rptno,
                 preweight, postweight, lostweight, lostweight_per,
                 totaltesttime, constpower, phenocode, flametime, flameduration,
                 maxtf1, maxtf2, maxts, maxtc,
                 maxtf1_time, maxtf2_time, maxts_time, maxtc_time,
                 finaltf1, finaltf2, finalts, finaltc,
                 finaltf1_time, finaltf2_time, finalts_time, finaltc_time,
                 deltatf1, deltatf2, deltatf, deltats, deltatc, memo, flag)
            VALUES
                ($pid, $tid, $tdate, $ambt, $ambh,
                 $acc, $op, $appid, $appname, $appchk, $rptno,
                 $prewt, 0, 0, 0,
                 0, 0, '', 0, 0,
                 0, 0, 0, 0, 0, 0, 0, 0,
                 0, 0, 0, 0, 0, 0, 0, 0,
                 0, 0, 0, 0, 0, $memo, $flag)";
        cmd.Parameters.AddWithValue("$pid", test.ProductId);
        cmd.Parameters.AddWithValue("$tid", test.TestId);
        cmd.Parameters.AddWithValue("$tdate", test.TestDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$ambt", test.AmbTemp);
        cmd.Parameters.AddWithValue("$ambh", test.AmbHumi);
        cmd.Parameters.AddWithValue("$acc", test.According);
        cmd.Parameters.AddWithValue("$op", test.Operator);
        cmd.Parameters.AddWithValue("$appid", test.ApparatusId);
        cmd.Parameters.AddWithValue("$appname", test.ApparatusName);
        cmd.Parameters.AddWithValue("$appchk", test.ApparatusChkDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$rptno", test.RptNo);
        cmd.Parameters.AddWithValue("$prewt", test.PreWeight);
        cmd.Parameters.AddWithValue("$memo", test.Memo ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$flag", test.Flag ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 试验完成后更新统计字段，设置 flag='10000000'
    /// </summary>
    public void UpdateTestResult(TestMaster test)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE testmaster SET
                postweight      = $post,
                lostweight      = $lost,
                lostweight_per  = $lostper,
                totaltesttime   = $time,
                constpower      = $cp,
                phenocode       = $pheno,
                flametime       = $ftime,
                flameduration   = $fdur,
                maxtf1          = $maxtf1,
                maxtf2          = $maxtf2,
                maxts           = $maxts,
                maxtc           = $maxtc,
                maxtf1_time     = $maxtf1t,
                maxtf2_time     = $maxtf2t,
                maxts_time      = $maxtst,
                maxtc_time      = $maxtct,
                finaltf1        = $ftf1,
                finaltf2        = $ftf2,
                finalts         = $fts,
                finaltc         = $ftc,
                finaltf1_time   = $ftf1t,
                finaltf2_time   = $ftf2t,
                finalts_time    = $ftst,
                finaltc_time    = $ftct,
                deltatf1        = $dtf1,
                deltatf2        = $dtf2,
                deltatf         = $dtf,
                deltats         = $dts,
                deltatc         = $dtc,
                memo            = $memo,
                flag            = '10000000'
            WHERE productid=$pid AND testid=$tid";
        cmd.Parameters.AddWithValue("$post", test.PostWeight);
        cmd.Parameters.AddWithValue("$lost", test.LostWeight);
        cmd.Parameters.AddWithValue("$lostper", test.LostWeightPer);
        cmd.Parameters.AddWithValue("$time", test.TotalTestTime);
        cmd.Parameters.AddWithValue("$cp", test.ConstPower);
        cmd.Parameters.AddWithValue("$pheno", test.PhenoCode);
        cmd.Parameters.AddWithValue("$ftime", test.FlameTime);
        cmd.Parameters.AddWithValue("$fdur", test.FlameDuration);
        cmd.Parameters.AddWithValue("$maxtf1", test.MaxTf1);
        cmd.Parameters.AddWithValue("$maxtf2", test.MaxTf2);
        cmd.Parameters.AddWithValue("$maxts", test.MaxTs);
        cmd.Parameters.AddWithValue("$maxtc", test.MaxTc);
        cmd.Parameters.AddWithValue("$maxtf1t", test.MaxTf1Time);
        cmd.Parameters.AddWithValue("$maxtf2t", test.MaxTf2Time);
        cmd.Parameters.AddWithValue("$maxtst", test.MaxTsTime);
        cmd.Parameters.AddWithValue("$maxtct", test.MaxTcTime);
        cmd.Parameters.AddWithValue("$ftf1", test.FinalTf1);
        cmd.Parameters.AddWithValue("$ftf2", test.FinalTf2);
        cmd.Parameters.AddWithValue("$fts", test.FinalTs);
        cmd.Parameters.AddWithValue("$ftc", test.FinalTc);
        cmd.Parameters.AddWithValue("$ftf1t", test.FinalTf1Time);
        cmd.Parameters.AddWithValue("$ftf2t", test.FinalTf2Time);
        cmd.Parameters.AddWithValue("$ftst", test.FinalTsTime);
        cmd.Parameters.AddWithValue("$ftct", test.FinalTcTime);
        cmd.Parameters.AddWithValue("$dtf1", test.DeltaTf1);
        cmd.Parameters.AddWithValue("$dtf2", test.DeltaTf2);
        cmd.Parameters.AddWithValue("$dtf", test.DeltaTf);
        cmd.Parameters.AddWithValue("$dts", test.DeltaTs);
        cmd.Parameters.AddWithValue("$dtc", test.DeltaTc);
        cmd.Parameters.AddWithValue("$memo", test.Memo ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$pid", test.ProductId);
        cmd.Parameters.AddWithValue("$tid", test.TestId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 获取当前活动试验（未保存的已完成试验，或最近一次试验）
    /// </summary>
    public TestMaster? GetCurrentTest()
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        // 优先返回 flag != '10000000' 且 totaltesttime > 0 的试验（已完成但未保存）
        cmd.CommandText = @"
            SELECT * FROM testmaster
            WHERE flag IS NULL OR flag != '10000000'
            ORDER BY testdate DESC, testid DESC
            LIMIT 1";

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return ReadTestMaster(reader);
        return null;
    }

    /// <summary>
    /// 按联合主键获取试验记录
    /// </summary>
    public TestMaster? GetTest(string productId, string testId)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM testmaster WHERE productid=$pid AND testid=$tid";
        cmd.Parameters.AddWithValue("$pid", productId);
        cmd.Parameters.AddWithValue("$tid", testId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return ReadTestMaster(reader);
        return null;
    }

    /// <summary>
    /// 按日期范围和条件查询试验历史
    /// </summary>
    public List<TestMaster> QueryTests(DateTime from, DateTime to, string? productId = null, string? operatorName = null)
    {
        var list = new List<TestMaster>();
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();

        var sql = @"
            SELECT * FROM testmaster
            WHERE testdate BETWEEN $from AND $to
              AND ($pid = '' OR productid LIKE '%' || $pid || '%')
              AND ($op = '' OR operator = $op)
            ORDER BY testdate DESC, testid DESC";
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("$from", from.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$to", to.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$pid", productId ?? string.Empty);
        cmd.Parameters.AddWithValue("$op", operatorName ?? string.Empty);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(ReadTestMaster(reader));
        return list;
    }

    /// <summary>
    /// 获取所有操作员列表（用于下拉筛选）
    /// </summary>
    public List<string> GetAllOperators()
    {
        var list = new List<string>();
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT DISTINCT username FROM operators ORDER BY username";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(reader.GetString(0));
        return list;
    }

    private static TestMaster ReadTestMaster(SqliteDataReader reader)
    {
        return new TestMaster
        {
            ProductId = reader.GetString(0),
            TestId = reader.GetString(1),
            TestDate = DateTime.Parse(reader.GetString(2)),
            AmbTemp = reader.GetDouble(3),
            AmbHumi = reader.GetDouble(4),
            According = reader.GetString(5),
            Operator = reader.GetString(6),
            ApparatusId = reader.GetString(7),
            ApparatusName = reader.GetString(8),
            ApparatusChkDate = DateTime.Parse(reader.GetString(9)),
            RptNo = reader.GetString(10),
            PreWeight = reader.GetDouble(11),
            PostWeight = reader.GetDouble(12),
            LostWeight = reader.GetDouble(13),
            LostWeightPer = reader.GetDouble(14),
            TotalTestTime = reader.GetInt32(15),
            ConstPower = reader.GetInt32(16),
            PhenoCode = reader.GetString(17),
            FlameTime = reader.GetInt32(18),
            FlameDuration = reader.GetInt32(19),
            MaxTf1 = reader.GetDouble(20),
            MaxTf2 = reader.GetDouble(21),
            MaxTs = reader.GetDouble(22),
            MaxTc = reader.GetDouble(23),
            MaxTf1Time = reader.GetInt32(24),
            MaxTf2Time = reader.GetInt32(25),
            MaxTsTime = reader.GetInt32(26),
            MaxTcTime = reader.GetInt32(27),
            FinalTf1 = reader.GetDouble(28),
            FinalTf2 = reader.GetDouble(29),
            FinalTs = reader.GetDouble(30),
            FinalTc = reader.GetDouble(31),
            FinalTf1Time = reader.GetInt32(32),
            FinalTf2Time = reader.GetInt32(33),
            FinalTsTime = reader.GetInt32(34),
            FinalTcTime = reader.GetInt32(35),
            DeltaTf1 = reader.GetDouble(36),
            DeltaTf2 = reader.GetDouble(37),
            DeltaTf = reader.GetDouble(38),
            DeltaTs = reader.GetDouble(39),
            DeltaTc = reader.GetDouble(40),
            Memo = reader.IsDBNull(41) ? null : reader.GetString(41),
            Flag = reader.IsDBNull(42) ? null : reader.GetString(42)
        };
    }

    // ================================================================
    // 校准记录
    // ================================================================

    /// <summary>
    /// 保存校准记录
    /// </summary>
    public void InsertCalibrationRecord(CalibrationRecord record)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO CalibrationRecords
                (Id, CalibrationDate, CalibrationType, ApparatusId, Operator,
                 TemperatureData, UniformityResult, MaxDeviation, AverageTemperature,
                 PassedCriteria, Remarks, CreatedAt,
                 TempA1, TempA2, TempA3, TempB1, TempB2, TempB3, TempC1, TempC2, TempC3,
                 TAvg, TAvgAxis1, TAvgAxis2, TAvgAxis3,
                 TAvgLevela, TAvgLevelb, TAvgLevelc,
                 TDevAxis1, TDevAxis2, TDevAxis3,
                 TDevLevela, TDevLevelb, TDevLevelc,
                 TAvgDevAxis, TAvgDevLevel, CenterTempData, Memo)
            VALUES
                ($id, $date, $type, $appid, $op,
                 $tempdata, $ur, $md, $at,
                 $pc, $rm, $ca,
                 $ta1, $ta2, $ta3, $tb1, $tb2, $tb3, $tc1, $tc2, $tc3,
                 $tavg, $tavga1, $tavga2, $tavga3,
                 $tavglA, $tavglB, $tavglC,
                 $tdeva1, $tdeva2, $tdeva3,
                 $tdevlA, $tdevlB, $tdevlC,
                 $tavgdevA, $tavgdevL, $ctd, $memo)";

        cmd.Parameters.AddWithValue("$id", record.Id);
        cmd.Parameters.AddWithValue("$date", record.CalibrationDate);
        cmd.Parameters.AddWithValue("$type", record.CalibrationType);
        cmd.Parameters.AddWithValue("$appid", record.ApparatusId);
        cmd.Parameters.AddWithValue("$op", record.Operator);
        cmd.Parameters.AddWithValue("$tempdata", record.TemperatureData);
        cmd.Parameters.AddWithValue("$ur", record.UniformityResult ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$md", record.MaxDeviation ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$at", record.AverageTemperature ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$pc", record.PassedCriteria);
        cmd.Parameters.AddWithValue("$rm", record.Remarks);
        cmd.Parameters.AddWithValue("$ca", record.CreatedAt);
        cmd.Parameters.AddWithValue("$ta1", record.TempA1 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$ta2", record.TempA2 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$ta3", record.TempA3 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tb1", record.TempB1 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tb2", record.TempB2 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tb3", record.TempB3 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tc1", record.TempC1 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tc2", record.TempC2 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tc3", record.TempC3 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tavg", record.TAvg ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tavga1", record.TAvgAxis1 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tavga2", record.TAvgAxis2 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tavga3", record.TAvgAxis3 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tavglA", record.TAvgLevela ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tavglB", record.TAvgLevelb ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tavglC", record.TAvgLevelc ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tdeva1", record.TDevAxis1 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tdeva2", record.TDevAxis2 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tdeva3", record.TDevAxis3 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tdevlA", record.TDevLevela ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tdevlB", record.TDevLevelb ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tdevlC", record.TDevLevelc ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tavgdevA", record.TAvgDevAxis ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tavgdevL", record.TAvgDevLevel ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$ctd", record.CenterTempData ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$memo", record.Memo ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 查询校准历史记录
    /// </summary>
    public List<CalibrationRecord> QueryCalibrationRecords(DateTime? from = null, DateTime? to = null, string? operatorName = null)
    {
        var list = new List<CalibrationRecord>();
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();

        var sql = @"
            SELECT * FROM CalibrationRecords
            WHERE ($fd = '' OR CalibrationDate >= $fd)
              AND ($td = '' OR CalibrationDate <= $td)
              AND ($op = '' OR Operator = $op)
            ORDER BY CalibrationDate DESC";
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("$fd", from?.ToString("yyyy-MM-dd") ?? string.Empty);
        cmd.Parameters.AddWithValue("$td", to?.ToString("yyyy-MM-dd") ?? string.Empty);
        cmd.Parameters.AddWithValue("$op", operatorName ?? string.Empty);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(ReadCalibrationRecord(reader));
        return list;
    }

    private static CalibrationRecord ReadCalibrationRecord(SqliteDataReader reader)
    {
        return new CalibrationRecord
        {
            Id = reader.GetString(0),
            CalibrationDate = reader.GetString(1),
            CalibrationType = reader.GetString(2),
            ApparatusId = reader.GetInt32(3),
            Operator = reader.GetString(4),
            TemperatureData = reader.GetString(5),
            UniformityResult = reader.IsDBNull(6) ? null : reader.GetDouble(6),
            MaxDeviation = reader.IsDBNull(7) ? null : reader.GetDouble(7),
            AverageTemperature = reader.IsDBNull(8) ? null : reader.GetDouble(8),
            PassedCriteria = reader.GetInt32(9),
            Remarks = reader.GetString(10),
            CreatedAt = reader.GetString(11),
            TempA1 = reader.IsDBNull(12) ? null : reader.GetDouble(12),
            TempA2 = reader.IsDBNull(13) ? null : reader.GetDouble(13),
            TempA3 = reader.IsDBNull(14) ? null : reader.GetDouble(14),
            TempB1 = reader.IsDBNull(15) ? null : reader.GetDouble(15),
            TempB2 = reader.IsDBNull(16) ? null : reader.GetDouble(16),
            TempB3 = reader.IsDBNull(17) ? null : reader.GetDouble(17),
            TempC1 = reader.IsDBNull(18) ? null : reader.GetDouble(18),
            TempC2 = reader.IsDBNull(19) ? null : reader.GetDouble(19),
            TempC3 = reader.IsDBNull(20) ? null : reader.GetDouble(20),
            TAvg = reader.IsDBNull(21) ? null : reader.GetDouble(21),
            TAvgAxis1 = reader.IsDBNull(22) ? null : reader.GetDouble(22),
            TAvgAxis2 = reader.IsDBNull(23) ? null : reader.GetDouble(23),
            TAvgAxis3 = reader.IsDBNull(24) ? null : reader.GetDouble(24),
            TAvgLevela = reader.IsDBNull(25) ? null : reader.GetDouble(25),
            TAvgLevelb = reader.IsDBNull(26) ? null : reader.GetDouble(26),
            TAvgLevelc = reader.IsDBNull(27) ? null : reader.GetDouble(27),
            TDevAxis1 = reader.IsDBNull(28) ? null : reader.GetDouble(28),
            TDevAxis2 = reader.IsDBNull(29) ? null : reader.GetDouble(29),
            TDevAxis3 = reader.IsDBNull(30) ? null : reader.GetDouble(30),
            TDevLevela = reader.IsDBNull(31) ? null : reader.GetDouble(31),
            TDevLevelb = reader.IsDBNull(32) ? null : reader.GetDouble(32),
            TDevLevelc = reader.IsDBNull(33) ? null : reader.GetDouble(33),
            TAvgDevAxis = reader.IsDBNull(34) ? null : reader.GetDouble(34),
            TAvgDevLevel = reader.IsDBNull(35) ? null : reader.GetDouble(35),
            CenterTempData = reader.IsDBNull(36) ? null : reader.GetString(36),
            Memo = reader.IsDBNull(37) ? null : reader.GetString(37)
        };
    }
}
