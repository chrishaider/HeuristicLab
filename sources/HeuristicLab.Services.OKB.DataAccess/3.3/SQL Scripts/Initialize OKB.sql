USE [HeuristicLab.OKB]
GO
/****** Object:  Table [dbo].[User]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
 CONSTRAINT [PK_User_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SolutionRepresentation]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SolutionRepresentation](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_SolutionRepresentation_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UQ_SolutionRepresentation_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemClass]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemClass](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_ProblemClass_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UQ_ProblemClass_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Platform]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Platform](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_Platform_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UQ_Platform_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Platform] ON
INSERT [dbo].[Platform] ([Id], [Name], [Description]) VALUES (1, N'HL 2.5', N'HeuristicLab 2.5')
INSERT [dbo].[Platform] ([Id], [Name], [Description]) VALUES (2, N'HL 3.3', N'HeuristicLab 3.3')
SET IDENTITY_INSERT [dbo].[Platform] OFF
/****** Object:  Table [dbo].[DataType]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataType](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SqlName] [nvarchar](200) NOT NULL,
	[ClrName] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_DataType_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[DataType] ON
INSERT [dbo].[DataType] ([Id], [SqlName], [ClrName]) VALUES (1, N'Int', N'System.Int32')
INSERT [dbo].[DataType] ([Id], [SqlName], [ClrName]) VALUES (2, N'Float', N'System.Double')
INSERT [dbo].[DataType] ([Id], [SqlName], [ClrName]) VALUES (3, N'Char', N'System.String')
INSERT [dbo].[DataType] ([Id], [SqlName], [ClrName]) VALUES (4, N'Blob', N'System.Object')
INSERT [dbo].[DataType] ([Id], [SqlName], [ClrName]) VALUES (5, N'BLOB', N'IOperator')
INSERT [dbo].[DataType] ([Id], [SqlName], [ClrName]) VALUES (6, N'Bit', N'System.Boolean')
SET IDENTITY_INSERT [dbo].[DataType] OFF
/****** Object:  Table [dbo].[Client]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Client](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
 CONSTRAINT [PK_Client_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AlgorithmClass]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AlgorithmClass](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_AlgorithmClass_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UQ_AlgorithmClass_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Algorithm]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Algorithm](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AlgorithmClassId] [bigint] NOT NULL,
	[PlatformId] [bigint] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_Algorithm_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UQ_Algorithm_Name_PlattformId] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[PlatformId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemCharacteristic]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemCharacteristic](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[DataTypeId] [bigint] NOT NULL,
 CONSTRAINT [PK_ProblemCharacteristic_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UQ_ProblemCharacteristic_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Problem]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Problem](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ProblemClassId] [bigint] NOT NULL,
	[PlatformId] [bigint] NOT NULL,
	[SolutionRepresentationId] [bigint] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_Problem_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UQ_Problem_Name_PlattformId] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[PlatformId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemParameter]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemParameter](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ProblemId] [bigint] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Alias] [nvarchar](200) NULL,
	[Description] [nvarchar](max) NULL,
	[DataTypeId] [bigint] NOT NULL,
 CONSTRAINT [PK_ProblemParameter_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UQ_ProblemParameter_Name_ProblemId] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[ProblemId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemData]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ProblemData](
	[ProblemId] [bigint] NOT NULL,
	[Data] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_ProblemData_ProblemId] PRIMARY KEY CLUSTERED 
(
	[ProblemId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ProblemCharacteristicStringValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemCharacteristicStringValue](
	[ProblemCharacteristicId] [bigint] NOT NULL,
	[ProblemId] [bigint] NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_ProblemCharacteristicStringValue_ProblemCharacteristicId_ProblemId] PRIMARY KEY CLUSTERED 
(
	[ProblemCharacteristicId] ASC,
	[ProblemId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemCharacteristicIntValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemCharacteristicIntValue](
	[ProblemCharacteristicId] [bigint] NOT NULL,
	[ProblemId] [bigint] NOT NULL,
	[Value] [int] NOT NULL,
 CONSTRAINT [PK_ProblemCharacteristicIntValue_ProblemCharacteristicId_ProblemId] PRIMARY KEY CLUSTERED 
(
	[ProblemCharacteristicId] ASC,
	[ProblemId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemCharacteristicFloatValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemCharacteristicFloatValue](
	[ProblemCharacteristicId] [bigint] NOT NULL,
	[ProblemId] [bigint] NOT NULL,
	[Value] [float] NOT NULL,
 CONSTRAINT [PK_ProblemCharacteristicFloatValue_ProblemCharacteristicId_ProblemId] PRIMARY KEY CLUSTERED 
(
	[ProblemCharacteristicId] ASC,
	[ProblemId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AlgorithmParameter]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AlgorithmParameter](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AlgorithmId] [bigint] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Alias] [nvarchar](200) NULL,
	[Description] [nvarchar](max) NULL,
	[DataTypeId] [bigint] NOT NULL,
 CONSTRAINT [PK_AlgorithmParameter_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UQ_AlgorithmParameter_Name_AlgorithmId] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[AlgorithmId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AlgorithmData]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AlgorithmData](
	[AlgorithmId] [bigint] NOT NULL,
	[Data] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_AlgorithmData_AlgorithmId] PRIMARY KEY CLUSTERED 
(
	[AlgorithmId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Result]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Result](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AlgorithmId] [bigint] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Alias] [nvarchar](200) NULL,
	[Description] [nvarchar](max) NULL,
	[DataTypeId] [bigint] NOT NULL,
 CONSTRAINT [PK_Result_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UQ_Result_Name_AlgorithmId] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[AlgorithmId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemUser]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemUser](
	[ProblemId] [bigint] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_ProblemUser_ProblemId_UserId] PRIMARY KEY CLUSTERED 
(
	[ProblemId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Experiment]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Experiment](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AlgorithmId] [bigint] NOT NULL,
	[ProblemId] [bigint] NOT NULL,
 CONSTRAINT [PK_Experiment_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Experiment_AlgorithmId_ProblemId] ON [dbo].[Experiment] 
(
	[AlgorithmId] ASC,
	[ProblemId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AlgorithmUser]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AlgorithmUser](
	[AlgorithmId] [bigint] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_AlgorithmUser_AlgorithmId_UserId] PRIMARY KEY CLUSTERED 
(
	[AlgorithmId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AlgorithmParameterStringValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AlgorithmParameterStringValue](
	[AlgorithmParameterId] [bigint] NOT NULL,
	[ExperimentId] [bigint] NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_AlgorithmParameterStringValue_ParameterId_ExperimentId] PRIMARY KEY CLUSTERED 
(
	[AlgorithmParameterId] ASC,
	[ExperimentId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AlgorithmParameterIntValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AlgorithmParameterIntValue](
	[AlgorithmParameterId] [bigint] NOT NULL,
	[ExperimentId] [bigint] NOT NULL,
	[Value] [int] NOT NULL,
 CONSTRAINT [PK_AlgorithmParameterIntValue_AlgorithmParameterId_ExperimentId] PRIMARY KEY CLUSTERED 
(
	[AlgorithmParameterId] ASC,
	[ExperimentId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AlgorithmParameterFloatValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AlgorithmParameterFloatValue](
	[AlgorithmParameterId] [bigint] NOT NULL,
	[ExperimentId] [bigint] NOT NULL,
	[Value] [float] NOT NULL,
 CONSTRAINT [PK_AlgorithmParameterFloatValue_AlgorithmParameterId_ExperimentId] PRIMARY KEY CLUSTERED 
(
	[AlgorithmParameterId] ASC,
	[ExperimentId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AlgorithmParameterBoolValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AlgorithmParameterBoolValue](
	[AlgorithmParameterId] [bigint] NOT NULL,
	[ExperimentId] [bigint] NOT NULL,
	[Value] [bit] NOT NULL,
 CONSTRAINT [PK_AlgorithmParameterBoolValue_AlgorithmParameterId_ExperimentId] PRIMARY KEY CLUSTERED 
(
	[AlgorithmParameterId] ASC,
	[ExperimentId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AlgorithmParameterBlobValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AlgorithmParameterBlobValue](
	[AlgorithmParameterId] [bigint] NOT NULL,
	[ExperimentId] [bigint] NOT NULL,
	[Value] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_AlgorithmParameterBlobValue_AlgorithmParameterId_ExperimentId] PRIMARY KEY CLUSTERED 
(
	[AlgorithmParameterId] ASC,
	[ExperimentId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ProblemParameterStringValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemParameterStringValue](
	[ProblemParameterId] [bigint] NOT NULL,
	[ExperimentId] [bigint] NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_ProblemParameterStringValue_ProblemParameterId_ExperimentId] PRIMARY KEY CLUSTERED 
(
	[ProblemParameterId] ASC,
	[ExperimentId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemParameterIntValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemParameterIntValue](
	[ProblemParameterId] [bigint] NOT NULL,
	[ExperimentId] [bigint] NOT NULL,
	[Value] [int] NOT NULL,
 CONSTRAINT [PK_ProblemParameterIntValue_ProblemParameterId_ExperimentId] PRIMARY KEY CLUSTERED 
(
	[ProblemParameterId] ASC,
	[ExperimentId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemParameterFloatValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemParameterFloatValue](
	[ProblemParameterId] [bigint] NOT NULL,
	[ExperimentId] [bigint] NOT NULL,
	[Value] [float] NOT NULL,
 CONSTRAINT [PK_ProblemParameterFloatValue_ProblemParameterId_ExperimentId] PRIMARY KEY CLUSTERED 
(
	[ProblemParameterId] ASC,
	[ExperimentId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemParameterBoolValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemParameterBoolValue](
	[ProblemParameterId] [bigint] NOT NULL,
	[ExperimentId] [bigint] NOT NULL,
	[Value] [bit] NOT NULL,
 CONSTRAINT [PK_ProblemParameterBoolValue_ProblemParameterId_ExperimentId] PRIMARY KEY CLUSTERED 
(
	[ProblemParameterId] ASC,
	[ExperimentId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemParameterBlobValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ProblemParameterBlobValue](
	[ProblemParameterId] [bigint] NOT NULL,
	[ExperimentId] [bigint] NOT NULL,
	[Value] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_ProblemParameterBlobValue_ProblemParameterId_ExperimentId] PRIMARY KEY CLUSTERED 
(
	[ProblemParameterId] ASC,
	[ExperimentId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Run]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Run](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ExperimentId] [bigint] NOT NULL,
	[RandomSeed] [int] NOT NULL,
	[FinishedDate] [datetime2](7) NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Run_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Run_ExperimentId] ON [dbo].[Run] 
(
	[ExperimentId] ASC
)
INCLUDE ( [Id],
[FinishedDate],
[UserId],
[ClientId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResultStringValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResultStringValue](
	[ResultId] [bigint] NOT NULL,
	[RunId] [bigint] NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_ResultStringValue_ResultId_RunId] PRIMARY KEY CLUSTERED 
(
	[ResultId] ASC,
	[RunId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResultIntValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResultIntValue](
	[ResultId] [bigint] NOT NULL,
	[RunId] [bigint] NOT NULL,
	[Value] [int] NOT NULL,
 CONSTRAINT [PK_ResultIntValue_ResultId_RunId] PRIMARY KEY CLUSTERED 
(
	[ResultId] ASC,
	[RunId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResultFloatValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResultFloatValue](
	[ResultId] [bigint] NOT NULL,
	[RunId] [bigint] NOT NULL,
	[Value] [float] NOT NULL,
 CONSTRAINT [PK_ResultFloatValue_ResultId_RunId] PRIMARY KEY CLUSTERED 
(
	[ResultId] ASC,
	[RunId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResultBoolValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResultBoolValue](
	[ResultId] [bigint] NOT NULL,
	[RunId] [bigint] NOT NULL,
	[Value] [bit] NOT NULL,
 CONSTRAINT [PK_ResultBoolValue_ResultId_RunId] PRIMARY KEY CLUSTERED 
(
	[ResultId] ASC,
	[RunId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResultBlobValue]    Script Date: 09/12/2010 03:49:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ResultBlobValue](
	[ResultId] [bigint] NOT NULL,
	[RunId] [bigint] NOT NULL,
	[Value] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_ResultBlobValue_ResultId_RunId] PRIMARY KEY CLUSTERED 
(
	[ResultId] ASC,
	[RunId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  ForeignKey [FK_AlgorithmClass_Algorithm]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[Algorithm]  WITH CHECK ADD  CONSTRAINT [FK_AlgorithmClass_Algorithm] FOREIGN KEY([AlgorithmClassId])
REFERENCES [dbo].[AlgorithmClass] ([Id])
GO
ALTER TABLE [dbo].[Algorithm] CHECK CONSTRAINT [FK_AlgorithmClass_Algorithm]
GO
/****** Object:  ForeignKey [FK_Platform_Algorithm]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[Algorithm]  WITH CHECK ADD  CONSTRAINT [FK_Platform_Algorithm] FOREIGN KEY([PlatformId])
REFERENCES [dbo].[Platform] ([Id])
GO
ALTER TABLE [dbo].[Algorithm] CHECK CONSTRAINT [FK_Platform_Algorithm]
GO
/****** Object:  ForeignKey [FK_Algorithm_AlgorithmData]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmData]  WITH CHECK ADD  CONSTRAINT [FK_Algorithm_AlgorithmData] FOREIGN KEY([AlgorithmId])
REFERENCES [dbo].[Algorithm] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmData] CHECK CONSTRAINT [FK_Algorithm_AlgorithmData]
GO
/****** Object:  ForeignKey [FK_Algorithm_AlgorithmParameter]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmParameter]  WITH CHECK ADD  CONSTRAINT [FK_Algorithm_AlgorithmParameter] FOREIGN KEY([AlgorithmId])
REFERENCES [dbo].[Algorithm] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmParameter] CHECK CONSTRAINT [FK_Algorithm_AlgorithmParameter]
GO
/****** Object:  ForeignKey [FK_DataType_AlgorithmParameter]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmParameter]  WITH CHECK ADD  CONSTRAINT [FK_DataType_AlgorithmParameter] FOREIGN KEY([DataTypeId])
REFERENCES [dbo].[DataType] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmParameter] CHECK CONSTRAINT [FK_DataType_AlgorithmParameter]
GO
/****** Object:  ForeignKey [FK_AlgorithmParameter_AlgorithmParameterBlobValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmParameterBlobValue]  WITH CHECK ADD  CONSTRAINT [FK_AlgorithmParameter_AlgorithmParameterBlobValue] FOREIGN KEY([AlgorithmParameterId])
REFERENCES [dbo].[AlgorithmParameter] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmParameterBlobValue] CHECK CONSTRAINT [FK_AlgorithmParameter_AlgorithmParameterBlobValue]
GO
/****** Object:  ForeignKey [FK_Experiment_AlgorithmParameterBlobValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmParameterBlobValue]  WITH CHECK ADD  CONSTRAINT [FK_Experiment_AlgorithmParameterBlobValue] FOREIGN KEY([ExperimentId])
REFERENCES [dbo].[Experiment] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmParameterBlobValue] CHECK CONSTRAINT [FK_Experiment_AlgorithmParameterBlobValue]
GO
/****** Object:  ForeignKey [FK_AlgorithmParameter_AlgorithmParameterBoolValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmParameterBoolValue]  WITH CHECK ADD  CONSTRAINT [FK_AlgorithmParameter_AlgorithmParameterBoolValue] FOREIGN KEY([AlgorithmParameterId])
REFERENCES [dbo].[AlgorithmParameter] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmParameterBoolValue] CHECK CONSTRAINT [FK_AlgorithmParameter_AlgorithmParameterBoolValue]
GO
/****** Object:  ForeignKey [FK_Experiment_AlgorithmParameterBoolValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmParameterBoolValue]  WITH CHECK ADD  CONSTRAINT [FK_Experiment_AlgorithmParameterBoolValue] FOREIGN KEY([ExperimentId])
REFERENCES [dbo].[Experiment] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmParameterBoolValue] CHECK CONSTRAINT [FK_Experiment_AlgorithmParameterBoolValue]
GO
/****** Object:  ForeignKey [FK_AlgorithmParameter_AlgorithmParameterFloatValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmParameterFloatValue]  WITH CHECK ADD  CONSTRAINT [FK_AlgorithmParameter_AlgorithmParameterFloatValue] FOREIGN KEY([AlgorithmParameterId])
REFERENCES [dbo].[AlgorithmParameter] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmParameterFloatValue] CHECK CONSTRAINT [FK_AlgorithmParameter_AlgorithmParameterFloatValue]
GO
/****** Object:  ForeignKey [FK_Experiment_AlgorithmParameterFloatValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmParameterFloatValue]  WITH CHECK ADD  CONSTRAINT [FK_Experiment_AlgorithmParameterFloatValue] FOREIGN KEY([ExperimentId])
REFERENCES [dbo].[Experiment] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmParameterFloatValue] CHECK CONSTRAINT [FK_Experiment_AlgorithmParameterFloatValue]
GO
/****** Object:  ForeignKey [FK_AlgorithmParameter_AlgorithmParameterIntValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmParameterIntValue]  WITH CHECK ADD  CONSTRAINT [FK_AlgorithmParameter_AlgorithmParameterIntValue] FOREIGN KEY([AlgorithmParameterId])
REFERENCES [dbo].[AlgorithmParameter] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmParameterIntValue] CHECK CONSTRAINT [FK_AlgorithmParameter_AlgorithmParameterIntValue]
GO
/****** Object:  ForeignKey [FK_Experiment_AlgorithmParameterIntValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmParameterIntValue]  WITH CHECK ADD  CONSTRAINT [FK_Experiment_AlgorithmParameterIntValue] FOREIGN KEY([ExperimentId])
REFERENCES [dbo].[Experiment] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmParameterIntValue] CHECK CONSTRAINT [FK_Experiment_AlgorithmParameterIntValue]
GO
/****** Object:  ForeignKey [FK_AlgorithmParameter_AlgorithmParameterStringValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmParameterStringValue]  WITH CHECK ADD  CONSTRAINT [FK_AlgorithmParameter_AlgorithmParameterStringValue] FOREIGN KEY([AlgorithmParameterId])
REFERENCES [dbo].[AlgorithmParameter] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmParameterStringValue] CHECK CONSTRAINT [FK_AlgorithmParameter_AlgorithmParameterStringValue]
GO
/****** Object:  ForeignKey [FK_Experiment_AlgorithmParameterStringValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmParameterStringValue]  WITH CHECK ADD  CONSTRAINT [FK_Experiment_AlgorithmParameterStringValue] FOREIGN KEY([ExperimentId])
REFERENCES [dbo].[Experiment] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmParameterStringValue] CHECK CONSTRAINT [FK_Experiment_AlgorithmParameterStringValue]
GO
/****** Object:  ForeignKey [FK_Algorithm_AlgorithmUser]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmUser]  WITH CHECK ADD  CONSTRAINT [FK_Algorithm_AlgorithmUser] FOREIGN KEY([AlgorithmId])
REFERENCES [dbo].[Algorithm] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmUser] CHECK CONSTRAINT [FK_Algorithm_AlgorithmUser]
GO
/****** Object:  ForeignKey [FK_User_AlgorithmUser]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[AlgorithmUser]  WITH CHECK ADD  CONSTRAINT [FK_User_AlgorithmUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[AlgorithmUser] CHECK CONSTRAINT [FK_User_AlgorithmUser]
GO
/****** Object:  ForeignKey [FK_Algorithm_Experiment]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[Experiment]  WITH CHECK ADD  CONSTRAINT [FK_Algorithm_Experiment] FOREIGN KEY([AlgorithmId])
REFERENCES [dbo].[Algorithm] ([Id])
GO
ALTER TABLE [dbo].[Experiment] CHECK CONSTRAINT [FK_Algorithm_Experiment]
GO
/****** Object:  ForeignKey [FK_Problem_Experiment]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[Experiment]  WITH CHECK ADD  CONSTRAINT [FK_Problem_Experiment] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problem] ([Id])
GO
ALTER TABLE [dbo].[Experiment] CHECK CONSTRAINT [FK_Problem_Experiment]
GO
/****** Object:  ForeignKey [FK_Platform_Problem]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[Problem]  WITH CHECK ADD  CONSTRAINT [FK_Platform_Problem] FOREIGN KEY([PlatformId])
REFERENCES [dbo].[Platform] ([Id])
GO
ALTER TABLE [dbo].[Problem] CHECK CONSTRAINT [FK_Platform_Problem]
GO
/****** Object:  ForeignKey [FK_ProblemClass_Problem]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[Problem]  WITH CHECK ADD  CONSTRAINT [FK_ProblemClass_Problem] FOREIGN KEY([ProblemClassId])
REFERENCES [dbo].[ProblemClass] ([Id])
GO
ALTER TABLE [dbo].[Problem] CHECK CONSTRAINT [FK_ProblemClass_Problem]
GO
/****** Object:  ForeignKey [FK_SolutionRepresentation_Problem]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[Problem]  WITH CHECK ADD  CONSTRAINT [FK_SolutionRepresentation_Problem] FOREIGN KEY([SolutionRepresentationId])
REFERENCES [dbo].[SolutionRepresentation] ([Id])
GO
ALTER TABLE [dbo].[Problem] CHECK CONSTRAINT [FK_SolutionRepresentation_Problem]
GO
/****** Object:  ForeignKey [FK_DataType_ProblemCharacteristic]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemCharacteristic]  WITH CHECK ADD  CONSTRAINT [FK_DataType_ProblemCharacteristic] FOREIGN KEY([DataTypeId])
REFERENCES [dbo].[DataType] ([Id])
GO
ALTER TABLE [dbo].[ProblemCharacteristic] CHECK CONSTRAINT [FK_DataType_ProblemCharacteristic]
GO
/****** Object:  ForeignKey [FK_Problem_ProblemCharacteristicFloatValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemCharacteristicFloatValue]  WITH CHECK ADD  CONSTRAINT [FK_Problem_ProblemCharacteristicFloatValue] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problem] ([Id])
GO
ALTER TABLE [dbo].[ProblemCharacteristicFloatValue] CHECK CONSTRAINT [FK_Problem_ProblemCharacteristicFloatValue]
GO
/****** Object:  ForeignKey [FK_ProblemCharacteristic_ProblemCharacteristicFloatValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemCharacteristicFloatValue]  WITH CHECK ADD  CONSTRAINT [FK_ProblemCharacteristic_ProblemCharacteristicFloatValue] FOREIGN KEY([ProblemCharacteristicId])
REFERENCES [dbo].[ProblemCharacteristic] ([Id])
GO
ALTER TABLE [dbo].[ProblemCharacteristicFloatValue] CHECK CONSTRAINT [FK_ProblemCharacteristic_ProblemCharacteristicFloatValue]
GO
/****** Object:  ForeignKey [FK_Problem_ProblemCharacteristicIntValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemCharacteristicIntValue]  WITH CHECK ADD  CONSTRAINT [FK_Problem_ProblemCharacteristicIntValue] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problem] ([Id])
GO
ALTER TABLE [dbo].[ProblemCharacteristicIntValue] CHECK CONSTRAINT [FK_Problem_ProblemCharacteristicIntValue]
GO
/****** Object:  ForeignKey [FK_ProblemCharacteristic_ProblemCharacteristicIntValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemCharacteristicIntValue]  WITH CHECK ADD  CONSTRAINT [FK_ProblemCharacteristic_ProblemCharacteristicIntValue] FOREIGN KEY([ProblemCharacteristicId])
REFERENCES [dbo].[ProblemCharacteristic] ([Id])
GO
ALTER TABLE [dbo].[ProblemCharacteristicIntValue] CHECK CONSTRAINT [FK_ProblemCharacteristic_ProblemCharacteristicIntValue]
GO
/****** Object:  ForeignKey [FK_Problem_ProblemCharacteristicStringValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemCharacteristicStringValue]  WITH CHECK ADD  CONSTRAINT [FK_Problem_ProblemCharacteristicStringValue] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problem] ([Id])
GO
ALTER TABLE [dbo].[ProblemCharacteristicStringValue] CHECK CONSTRAINT [FK_Problem_ProblemCharacteristicStringValue]
GO
/****** Object:  ForeignKey [FK_ProblemCharacteristic_ProblemCharacteristicStringValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemCharacteristicStringValue]  WITH CHECK ADD  CONSTRAINT [FK_ProblemCharacteristic_ProblemCharacteristicStringValue] FOREIGN KEY([ProblemCharacteristicId])
REFERENCES [dbo].[ProblemCharacteristic] ([Id])
GO
ALTER TABLE [dbo].[ProblemCharacteristicStringValue] CHECK CONSTRAINT [FK_ProblemCharacteristic_ProblemCharacteristicStringValue]
GO
/****** Object:  ForeignKey [FK_Problem_ProblemData]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemData]  WITH CHECK ADD  CONSTRAINT [FK_Problem_ProblemData] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problem] ([Id])
GO
ALTER TABLE [dbo].[ProblemData] CHECK CONSTRAINT [FK_Problem_ProblemData]
GO
/****** Object:  ForeignKey [FK_DataType_ProblemParameter]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemParameter]  WITH CHECK ADD  CONSTRAINT [FK_DataType_ProblemParameter] FOREIGN KEY([DataTypeId])
REFERENCES [dbo].[DataType] ([Id])
GO
ALTER TABLE [dbo].[ProblemParameter] CHECK CONSTRAINT [FK_DataType_ProblemParameter]
GO
/****** Object:  ForeignKey [FK_Problem_ProblemParameter]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemParameter]  WITH CHECK ADD  CONSTRAINT [FK_Problem_ProblemParameter] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problem] ([Id])
GO
ALTER TABLE [dbo].[ProblemParameter] CHECK CONSTRAINT [FK_Problem_ProblemParameter]
GO
/****** Object:  ForeignKey [FK_Experiment_ProblemParameterBlobValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemParameterBlobValue]  WITH CHECK ADD  CONSTRAINT [FK_Experiment_ProblemParameterBlobValue] FOREIGN KEY([ExperimentId])
REFERENCES [dbo].[Experiment] ([Id])
GO
ALTER TABLE [dbo].[ProblemParameterBlobValue] CHECK CONSTRAINT [FK_Experiment_ProblemParameterBlobValue]
GO
/****** Object:  ForeignKey [FK_ProblemParameter_ProblemParameterBlobValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemParameterBlobValue]  WITH CHECK ADD  CONSTRAINT [FK_ProblemParameter_ProblemParameterBlobValue] FOREIGN KEY([ProblemParameterId])
REFERENCES [dbo].[ProblemParameter] ([Id])
GO
ALTER TABLE [dbo].[ProblemParameterBlobValue] CHECK CONSTRAINT [FK_ProblemParameter_ProblemParameterBlobValue]
GO
/****** Object:  ForeignKey [FK_Experiment_ProblemParameterBoolValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemParameterBoolValue]  WITH CHECK ADD  CONSTRAINT [FK_Experiment_ProblemParameterBoolValue] FOREIGN KEY([ExperimentId])
REFERENCES [dbo].[Experiment] ([Id])
GO
ALTER TABLE [dbo].[ProblemParameterBoolValue] CHECK CONSTRAINT [FK_Experiment_ProblemParameterBoolValue]
GO
/****** Object:  ForeignKey [FK_ProblemParameter_ProblemParameterBoolValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemParameterBoolValue]  WITH CHECK ADD  CONSTRAINT [FK_ProblemParameter_ProblemParameterBoolValue] FOREIGN KEY([ProblemParameterId])
REFERENCES [dbo].[ProblemParameter] ([Id])
GO
ALTER TABLE [dbo].[ProblemParameterBoolValue] CHECK CONSTRAINT [FK_ProblemParameter_ProblemParameterBoolValue]
GO
/****** Object:  ForeignKey [FK_Experiment_ProblemParameterFloatValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemParameterFloatValue]  WITH CHECK ADD  CONSTRAINT [FK_Experiment_ProblemParameterFloatValue] FOREIGN KEY([ExperimentId])
REFERENCES [dbo].[Experiment] ([Id])
GO
ALTER TABLE [dbo].[ProblemParameterFloatValue] CHECK CONSTRAINT [FK_Experiment_ProblemParameterFloatValue]
GO
/****** Object:  ForeignKey [FK_ProblemParameter_ProblemParameterFloatValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemParameterFloatValue]  WITH CHECK ADD  CONSTRAINT [FK_ProblemParameter_ProblemParameterFloatValue] FOREIGN KEY([ProblemParameterId])
REFERENCES [dbo].[ProblemParameter] ([Id])
GO
ALTER TABLE [dbo].[ProblemParameterFloatValue] CHECK CONSTRAINT [FK_ProblemParameter_ProblemParameterFloatValue]
GO
/****** Object:  ForeignKey [FK_Experiment_ProblemParameterIntValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemParameterIntValue]  WITH CHECK ADD  CONSTRAINT [FK_Experiment_ProblemParameterIntValue] FOREIGN KEY([ExperimentId])
REFERENCES [dbo].[Experiment] ([Id])
GO
ALTER TABLE [dbo].[ProblemParameterIntValue] CHECK CONSTRAINT [FK_Experiment_ProblemParameterIntValue]
GO
/****** Object:  ForeignKey [FK_ProblemParameter_ProblemParameterIntValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemParameterIntValue]  WITH CHECK ADD  CONSTRAINT [FK_ProblemParameter_ProblemParameterIntValue] FOREIGN KEY([ProblemParameterId])
REFERENCES [dbo].[ProblemParameter] ([Id])
GO
ALTER TABLE [dbo].[ProblemParameterIntValue] CHECK CONSTRAINT [FK_ProblemParameter_ProblemParameterIntValue]
GO
/****** Object:  ForeignKey [FK_Experiment_ProblemParameterStringValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemParameterStringValue]  WITH CHECK ADD  CONSTRAINT [FK_Experiment_ProblemParameterStringValue] FOREIGN KEY([ExperimentId])
REFERENCES [dbo].[Experiment] ([Id])
GO
ALTER TABLE [dbo].[ProblemParameterStringValue] CHECK CONSTRAINT [FK_Experiment_ProblemParameterStringValue]
GO
/****** Object:  ForeignKey [FK_ProblemParameter_ProblemParameterStringValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemParameterStringValue]  WITH CHECK ADD  CONSTRAINT [FK_ProblemParameter_ProblemParameterStringValue] FOREIGN KEY([ProblemParameterId])
REFERENCES [dbo].[ProblemParameter] ([Id])
GO
ALTER TABLE [dbo].[ProblemParameterStringValue] CHECK CONSTRAINT [FK_ProblemParameter_ProblemParameterStringValue]
GO
/****** Object:  ForeignKey [FK_Problem_ProblemUser]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemUser]  WITH CHECK ADD  CONSTRAINT [FK_Problem_ProblemUser] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problem] ([Id])
GO
ALTER TABLE [dbo].[ProblemUser] CHECK CONSTRAINT [FK_Problem_ProblemUser]
GO
/****** Object:  ForeignKey [FK_User_ProblemUser]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ProblemUser]  WITH CHECK ADD  CONSTRAINT [FK_User_ProblemUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[ProblemUser] CHECK CONSTRAINT [FK_User_ProblemUser]
GO
/****** Object:  ForeignKey [FK_Algorithm_Result]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[Result]  WITH CHECK ADD  CONSTRAINT [FK_Algorithm_Result] FOREIGN KEY([AlgorithmId])
REFERENCES [dbo].[Algorithm] ([Id])
GO
ALTER TABLE [dbo].[Result] CHECK CONSTRAINT [FK_Algorithm_Result]
GO
/****** Object:  ForeignKey [FK_DataType_Result]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[Result]  WITH CHECK ADD  CONSTRAINT [FK_DataType_Result] FOREIGN KEY([DataTypeId])
REFERENCES [dbo].[DataType] ([Id])
GO
ALTER TABLE [dbo].[Result] CHECK CONSTRAINT [FK_DataType_Result]
GO
/****** Object:  ForeignKey [FK_Result_ResultBlobValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ResultBlobValue]  WITH CHECK ADD  CONSTRAINT [FK_Result_ResultBlobValue] FOREIGN KEY([ResultId])
REFERENCES [dbo].[Result] ([Id])
GO
ALTER TABLE [dbo].[ResultBlobValue] CHECK CONSTRAINT [FK_Result_ResultBlobValue]
GO
/****** Object:  ForeignKey [FK_Run_ResultBlobValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ResultBlobValue]  WITH CHECK ADD  CONSTRAINT [FK_Run_ResultBlobValue] FOREIGN KEY([RunId])
REFERENCES [dbo].[Run] ([Id])
GO
ALTER TABLE [dbo].[ResultBlobValue] CHECK CONSTRAINT [FK_Run_ResultBlobValue]
GO
/****** Object:  ForeignKey [FK_Result_ResultBoolValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ResultBoolValue]  WITH CHECK ADD  CONSTRAINT [FK_Result_ResultBoolValue] FOREIGN KEY([ResultId])
REFERENCES [dbo].[Result] ([Id])
GO
ALTER TABLE [dbo].[ResultBoolValue] CHECK CONSTRAINT [FK_Result_ResultBoolValue]
GO
/****** Object:  ForeignKey [FK_Run_ResultBoolValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ResultBoolValue]  WITH CHECK ADD  CONSTRAINT [FK_Run_ResultBoolValue] FOREIGN KEY([RunId])
REFERENCES [dbo].[Run] ([Id])
GO
ALTER TABLE [dbo].[ResultBoolValue] CHECK CONSTRAINT [FK_Run_ResultBoolValue]
GO
/****** Object:  ForeignKey [FK_Result_ResultFloatValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ResultFloatValue]  WITH CHECK ADD  CONSTRAINT [FK_Result_ResultFloatValue] FOREIGN KEY([ResultId])
REFERENCES [dbo].[Result] ([Id])
GO
ALTER TABLE [dbo].[ResultFloatValue] CHECK CONSTRAINT [FK_Result_ResultFloatValue]
GO
/****** Object:  ForeignKey [FK_Run_ResultFloatValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ResultFloatValue]  WITH CHECK ADD  CONSTRAINT [FK_Run_ResultFloatValue] FOREIGN KEY([RunId])
REFERENCES [dbo].[Run] ([Id])
GO
ALTER TABLE [dbo].[ResultFloatValue] CHECK CONSTRAINT [FK_Run_ResultFloatValue]
GO
/****** Object:  ForeignKey [FK_Result_ResultIntValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ResultIntValue]  WITH CHECK ADD  CONSTRAINT [FK_Result_ResultIntValue] FOREIGN KEY([ResultId])
REFERENCES [dbo].[Result] ([Id])
GO
ALTER TABLE [dbo].[ResultIntValue] CHECK CONSTRAINT [FK_Result_ResultIntValue]
GO
/****** Object:  ForeignKey [FK_Run_ResultIntValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ResultIntValue]  WITH CHECK ADD  CONSTRAINT [FK_Run_ResultIntValue] FOREIGN KEY([RunId])
REFERENCES [dbo].[Run] ([Id])
GO
ALTER TABLE [dbo].[ResultIntValue] CHECK CONSTRAINT [FK_Run_ResultIntValue]
GO
/****** Object:  ForeignKey [FK_Result_ResultStringValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ResultStringValue]  WITH CHECK ADD  CONSTRAINT [FK_Result_ResultStringValue] FOREIGN KEY([ResultId])
REFERENCES [dbo].[Result] ([Id])
GO
ALTER TABLE [dbo].[ResultStringValue] CHECK CONSTRAINT [FK_Result_ResultStringValue]
GO
/****** Object:  ForeignKey [FK_Run_ResultStringValue]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[ResultStringValue]  WITH CHECK ADD  CONSTRAINT [FK_Run_ResultStringValue] FOREIGN KEY([RunId])
REFERENCES [dbo].[Run] ([Id])
GO
ALTER TABLE [dbo].[ResultStringValue] CHECK CONSTRAINT [FK_Run_ResultStringValue]
GO
/****** Object:  ForeignKey [FK_Client_Run]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[Run]  WITH CHECK ADD  CONSTRAINT [FK_Client_Run] FOREIGN KEY([ClientId])
REFERENCES [dbo].[Client] ([Id])
GO
ALTER TABLE [dbo].[Run] CHECK CONSTRAINT [FK_Client_Run]
GO
/****** Object:  ForeignKey [FK_Experiment_Run]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[Run]  WITH CHECK ADD  CONSTRAINT [FK_Experiment_Run] FOREIGN KEY([ExperimentId])
REFERENCES [dbo].[Experiment] ([Id])
GO
ALTER TABLE [dbo].[Run] CHECK CONSTRAINT [FK_Experiment_Run]
GO
/****** Object:  ForeignKey [FK_User_Run]    Script Date: 09/12/2010 03:49:12 ******/
ALTER TABLE [dbo].[Run]  WITH CHECK ADD  CONSTRAINT [FK_User_Run] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[Run] CHECK CONSTRAINT [FK_User_Run]
GO
