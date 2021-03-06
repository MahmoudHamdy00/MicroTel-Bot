/****** Object:  Table [dbo].[bill]    Script Date: 9/6/2021 2:59:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[bill](
	[id] [uniqueidentifier] NOT NULL,
	[dueDate] [date] NOT NULL,
	[amount] [money] NOT NULL,
	[phoneNumber] [varchar](50) NULL,
	[isPaid] [int] NOT NULL,
	[teirID] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[extra_package]    Script Date: 9/6/2021 2:59:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[extra_package](
	[id] [uniqueidentifier] NOT NULL,
	[phoneNumber] [varchar](50) NULL,
	[extraPackageID] [uniqueidentifier] NULL,
	[date] [date] NULL,
	[totalPrice] [money] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[extra_package_details]    Script Date: 9/6/2021 2:59:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[extra_package_details](
	[id] [uniqueidentifier] NOT NULL,
	[name] [varchar](30) NULL,
	[minutes] [int] NULL,
	[messages] [int] NULL,
	[megabytes] [int] NULL,
	[price] [money] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[line]    Script Date: 9/6/2021 2:59:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[line](
	[phoneNumber] [varchar](50) NOT NULL,
	[tierID] [uniqueidentifier] NULL,
	[quotaID] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[phoneNumber] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[quotaID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[payment]    Script Date: 9/6/2021 2:59:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[payment](
	[id] [uniqueidentifier] NOT NULL,
	[date] [date] NOT NULL,
	[amount] [money] NOT NULL,
	[creditCard] [varchar](50) NOT NULL,
	[billID] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[quota]    Script Date: 9/6/2021 2:59:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[quota](
	[id] [uniqueidentifier] NOT NULL,
	[remainingMinutes] [int] NOT NULL,
	[remainingMessages] [int] NOT NULL,
	[remainingMegabytes] [int] NOT NULL,
	[date] [date] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tier_details]    Script Date: 9/6/2021 2:59:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tier_details](
	[id] [uniqueidentifier] NOT NULL,
	[name] [varchar](50) NOT NULL,
	[minutes] [int] NOT NULL,
	[messages] [int] NOT NULL,
	[megabytes] [int] NOT NULL,
	[price] [money] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[user]    Script Date: 9/6/2021 2:59:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[user](
	[timestamp] [datetime] NULL,
	[nationalID] [int] NOT NULL,
	[fName] [varchar](50) NOT NULL,
	[lName] [varchar](50) NOT NULL,
	[birthDate] [date] NOT NULL,
	[streetNo] [int] NOT NULL,
	[streetName] [varchar](50) NOT NULL,
	[city] [varchar](50) NOT NULL,
	[country] [varchar](50) NOT NULL,
	[phoneNumber] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[nationalID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[bill] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[bill] ADD  DEFAULT ((0)) FOR [isPaid]
GO
ALTER TABLE [dbo].[extra_package] ADD  CONSTRAINT [DF_ExtraPackage_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[extra_package_details] ADD  CONSTRAINT [DF_ExtraPackageDetails_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[payment] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[quota] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[tier_details] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[user] ADD  DEFAULT (getdate()) FOR [timestamp]
GO
ALTER TABLE [dbo].[bill]  WITH CHECK ADD FOREIGN KEY([phoneNumber])
REFERENCES [dbo].[line] ([phoneNumber])
GO
ALTER TABLE [dbo].[bill]  WITH CHECK ADD FOREIGN KEY([teirID])
REFERENCES [dbo].[tier_details] ([id])
GO
ALTER TABLE [dbo].[extra_package]  WITH CHECK ADD FOREIGN KEY([extraPackageID])
REFERENCES [dbo].[extra_package_details] ([id])
GO
ALTER TABLE [dbo].[extra_package]  WITH CHECK ADD FOREIGN KEY([phoneNumber])
REFERENCES [dbo].[line] ([phoneNumber])
GO
ALTER TABLE [dbo].[line]  WITH CHECK ADD FOREIGN KEY([quotaID])
REFERENCES [dbo].[quota] ([id])
GO
ALTER TABLE [dbo].[line]  WITH CHECK ADD FOREIGN KEY([tierID])
REFERENCES [dbo].[tier_details] ([id])
GO
ALTER TABLE [dbo].[payment]  WITH CHECK ADD FOREIGN KEY([billID])
REFERENCES [dbo].[bill] ([id])
GO
ALTER TABLE [dbo].[user]  WITH CHECK ADD FOREIGN KEY([phoneNumber])
REFERENCES [dbo].[line] ([phoneNumber])
GO
INSERT INTO [dbo].[tier_details] ([name], [minutes], [messages], [megabytes], [price])
VALUES ('Standard', 1000, 500, 10000, 10), ('Premium', 5000, 2500, 50000, 30), 
('VIP', 10000, 5000, 100000, 50);
GO
ALTER DATABASE [microtel-db] SET  READ_WRITE 
GO
