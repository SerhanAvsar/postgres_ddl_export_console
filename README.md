Programın Dosyalarının Tasarımı Son Halde Bu Şekilde Yapmaya Çalışacağım


PgDdlExporter
│
├── Program.cs                                                     
│
├── appsettings.json                    // Bağlantı string'i, export path, log ayarları                     ✓
├── .gitignore                          
│
├── Config
│   ├── DatabaseConfig.cs                                                                                   ✓
│   └── ExportConfig.cs                 // POCO: OutputPath, hangi nesne tipleri export edilecek            ✓
│
├── Models
│   ├── DatabaseObject.cs               
│   ├── TableInfo.cs
│   ├── ColumnInfo.cs
│   ├── ViewInfo.cs
│   ├── FunctionInfo.cs
│   ├── ProcedureInfo.cs
│   ├── SchemaInfo.cs
│   ├── TriggerInfo.cs
│   ├── SequenceInfo.cs
│   ├── IndexInfo.cs
│   ├── ConstraintInfo.cs
│   └── PgTypeInfo.cs                   
│
├── Queries                             
│   ├── TableQueries.cs
│   ├── ColumnQueries.cs
│   ├── ViewQueries.cs
│   ├── FunctionQueries.cs
│   ├── ProcedureQueries.cs
│   ├── TriggerQueries.cs
│   ├── IndexQueries.cs
│   ├── ConstraintQueries.cs
│   ├── SequenceQueries.cs
│   ├── SchemaQueries.cs
│   └── PgTypeQueries.cs
│
├── DdlBuilders                         
│   ├── IDdlBuilder.cs
│   ├── TableDdlBuilder.cs
│   ├── ViewDdlBuilder.cs               
│   ├── FunctionDdlBuilder.cs           
│   ├── ProcedureDdlBuilder.cs
│   ├── TriggerDdlBuilder.cs            
│   ├── IndexDdlBuilder.cs              
│   ├── ConstraintDdlBuilder.cs         
│   ├── SequenceDdlBuilder.cs
│   ├── SchemaDdlBuilder.cs
│   └── PgTypeDdlBuilder.cs
│
├── Services
│   ├── ConnectionService.cs
│   ├── DirectoryService.cs
│   ├── FileService.cs
│   ├── SqlFormatter.cs                 
│   ├── ExportOrderResolver.cs          
│   └── ExportManager.cs                
│
├── Exporters                           
│   ├── IExporter.cs
│   ├── SchemaExporter.cs
│   ├── TableExporter.cs
│   ├── ViewExporter.cs
│   ├── FunctionExporter.cs
│   ├── ProcedureExporter.cs
│   ├── TriggerExporter.cs
│   ├── IndexExporter.cs
│   ├── ConstraintExporter.cs
│   ├── SequenceExporter.cs
│   └── PgTypeExporter.cs
│
├── Extensions                          
│   └── NpgsqlDataReaderExtensions.cs   
│
├── Helpers
│   ├── Logger.cs                       
│   ├── PathHelper.cs
│   └── SqlHelper.cs
│
└── ExportedDDL/                        
    ├── 00_Schemas/
    ├── 01_Types/
    ├── 02_Sequences/
    ├── 03_Tables/
    ├── 04_Constraints/
    ├── 05_Indexes/
    ├── 06_Views/
    ├── 07_Functions/
    ├── 08_Procedures/
    └── 09_Triggers/