*** TODO ***
-----------------------------------------------------------

*	stateless service
*	stateful service
*	reliable actor
*	swagger ui
*	RPC communication
*	service startup ordering

-----------------------------------------------------------

*	partitioning
*	leaderboard
*	test this out
*	authentication
*	test this out

-----------------------------------------------------------

*	make relevant apis (reverse proxy?)
*	make frontend
*	make websockets (reverse proxy?)
*	guest exe

-----------------------------------------------------------

!!	custom metrics (load balancing, resource governance)
!!	affinity (placement contraints)
!!	auto-scaling (and overload)
!!	upgrade and rollback (service versioning)
!!	reverse proxy
**	deploy to the cloud

-----------------------------------------------------------

**	backup and restore
//	chaos testing (chaos monkey)
//	services coupling (orchestration)

-----------------------------------------------------------

Custom Metrics and Load Balancing
Auto-Scaling and Overload Handling
Service Affinity and Placement Constraints
Application Upgrades and Rollback
Service Coupling and Orchestration

Health Monitoring and Automatic Repair
Backup and Restore Mechanism
Security Enhancements with Managed Identity
SignalR Optimization and Real-Time Updates
Deployment to Cloud

-----------------------------------------------------------

*	documentation
*	presentation
*	demo

-----------------------------------------------------------

cd OnboardingApplication/StockTradingBot
python -m venv venv
venv\Scripts\activate
pip install requests
pip install pyinstaller
pyinstaller --onefile bot.py

cd OnboardingApplication/StockTradingClient
npm install
ng serve --open
ng build --configuration=production

cd OnboardingApplication/StockTradingPlatform
dotnet restore
dotnet build
dotnet publish

----------------------------------------------------------



*** PROMPTS ***
-----------------------------------------------------------

Hello brother, hope you are doing good! :)

I'm doing SF (Service Fabric) project regarding Stock Trading Market.
Idea is to include and see as many as possible SF features: stateless services, stateful services, their partitioning, reliable actors and reliable colletions, API services, guest executables, RPC (Remote Procedure Calling) for cluster-internal communication, SignalR (web socket communication) from cluster to external clients... I also want to integrate and front-end application into SF as an service, so the whole application is being run on the cluster itself. Firstly, I want to run all the services locally on 5-nodes cluster, and on the very end I want to deploy this application onto cloud. I also want to include some specifics: reverse proxy, custom metrics, affinity, auto-scalling (and kind of overload that will initiate the scaling), application upgrade and possible rollback, services coupling...

I've done a pretty decent amount of job, and the next thing to be done is making these SF specific things: reverse proxy, custom metrics, affinity, auto-scalling (and overload that will initiate the scaling, or whatever other way is used to iniitate the scaling), application upgrade and possible rollback, services coupling... Before I provide you with the current functionalities, let's see if you have any other advice on what specific SF thing I should apply in my application? :)

-----------------------------------------------------------

Here are the functionalities:

1)	[Stateless] Aggregator: each minute, it fetches the latest stock prices for 20 most popular symbols (AAPL, MSFT, GOOG...)
2)	[Stateless] Authentication: used for registration, login and later JWT processing
3)	[Stateless] Leaderboard: each 10 seconds updates the leaderboard, ranking and sorting users based on their portfolio
4)	[Stateless API] AngularClient: Angular application running on cluster itself
5)	[Stateless API] PlatformAPI: API service, where clients can send their requests from the client
	*** this service basically rerouts the requests to the other services
	--> AuthController: Register, Login, ValidateToken
	--> LeaderboardController: GetLeaderboard
	--> StockPricesController: GetLatestStockPrices, GetSupportedStockSymbols
	--> TradingController: BuyStock, SellStock, GetTransactionHistory
	--> UserManagementController: GetUserProfile
6)	[GuestExe] PythonBot: simulating the registration and login of 3 users, after which it periodically buys and sells stocks
7)	[Stateful] Trading: uses ReliableQueue to store orders; once order is received, it handles it appropriatelly
8)	[Stateful] UserProfile: uses ReliableDictionary to store info about users, creates few initial users (since we're not using anydatabase to keep users) and creates their initial portfolio
9)	[ReliableActor] UserPortfolio: keeps info about user portfolio
10)	[Stateless SignalR] RealTimeBroadcaster: broadcasts the desired information to all the connected clients (either aggregated latest stock prices or latest leaderboard state)

-----------------------------------------------------------

cd C:\Users\pesicpredrag\OneDrive - Microsoft\Desktop\OnboardingApplication\StockTradingPlatform
dotnet clean StockTradingPlatform.sln --configuration Release
dotnet build StockTradingPlatform.sln --configuration Release
cd StockTradingPlatform
dotnet publish StockTradingPlatform.sfproj --configuration Release --output ./pkg/Release

Connect-ServiceFabricCluster
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath "C:\Users\pesicpredrag\OneDrive - Microsoft\Desktop\OnboardingApplication\StockTradingPlatform\StockTradingPlatform\pkg\Release"
Register-ServiceFabricApplicationType -ApplicationPathInImageStore "Release"
New-ServiceFabricApplication -ApplicationName fabric:/StockTradingPlatform -ApplicationTypeName StockTradingPlatformType -ApplicationTypeVersion 1.0.0

Connect-ServiceFabricCluster
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath "C:\Users\pesicpredrag\OneDrive - Microsoft\Desktop\OnboardingApplication\StockTradingPlatform\StockTradingPlatform\pkg\Release"
Register-ServiceFabricApplicationType -ApplicationPathInImageStore "Release"
Start-ServiceFabricApplicationUpgrade -ApplicationName fabric:/StockTradingPlatform -ApplicationTypeVersion "1.0.1" -Monitored

Remove-ServiceFabricApplication -ApplicationName fabric:/StockTradingPlatform
Unregister-ServiceFabricApplicationType -ApplicationTypeName StockTradingPlatformType -ApplicationTypeVersion 1.0.0

Rollback-ServiceFabricApplicationUpgrade -ApplicationName fabric:/StockTradingPlatform


---------------------------

<DefaultServices>
  <!-- AngularClient Stateless Service -->
  <Service Name="AngularClient" ServicePackageActivationMode="ExclusiveProcess">
    <StatelessService ServiceTypeName="AngularClientType">
      <SingletonPartition />
    </StatelessService>
  </Service>
  <!-- PythonBot Stateless Service -->
  <Service Name="PythonBot" ServicePackageActivationMode="ExclusiveProcess">
    <StatelessService ServiceTypeName="PythonBotType">
      <SingletonPartition />
    </StatelessService>
  </Service>
  <!-- TRealTimeBroadcaster Stateless Service -->
  <Service Name="TRealTimeBroadcaster" ServicePackageActivationMode="ExclusiveProcess">
    <StatelessService ServiceTypeName="TRealTimeBroadcasterType">
      <SingletonPartition />
    </StatelessService>
  </Service>
  <!-- PlatformAPIV2 Stateless Service -->
  <Service Name="PlatformAPIV2" ServicePackageActivationMode="ExclusiveProcess">
    <StatelessService ServiceTypeName="PlatformAPIV2Type">
      <SingletonPartition />
    </StatelessService>
  </Service>
  <!-- Authentication Stateless Service -->
  <Service Name="Authentication" ServicePackageActivationMode="ExclusiveProcess">
    <StatelessService ServiceTypeName="AuthenticationType">
      <SingletonPartition />
    </StatelessService>
  </Service>
  <!-- LeaderboardStateless Stateless Service -->
  <Service Name="LeaderboardStateless" ServicePackageActivationMode="ExclusiveProcess">
    <StatelessService ServiceTypeName="LeaderboardStatelessType">
      <SingletonPartition />
    </StatelessService>
  </Service>
  <!-- TradingStateful Stateful Service -->
  <Service Name="TradingStateful" ServicePackageActivationMode="ExclusiveProcess">
    <StatefulService ServiceTypeName="TradingStatefulType">
      <NamedPartition>
        <Partition Name="SRB" />
        <Partition Name="MNE" />
        <Partition Name="BIH" />
      </NamedPartition>
    </StatefulService>
  </Service>
  <!-- UserProfile Stateful Service -->
  <Service Name="UserProfile" ServicePackageActivationMode="ExclusiveProcess">
    <StatefulService ServiceTypeName="UserProfileType" TargetReplicaSetSize="3" MinReplicaSetSize="3">
      <UniformInt64Partition PartitionCount="1" LowKey="-9223372036854775808" HighKey="9223372036854775807" />
    </StatefulService>
  </Service>
  <!-- Aggregator Stateless Service -->
  <Service Name="Aggregator" ServicePackageActivationMode="ExclusiveProcess">
    <StatelessService ServiceTypeName="AggregatorType">
      <SingletonPartition />
    </StatelessService>
  </Service>
  <!-- UserPortfolioActorService Stateful Service -->
  <Service Name="UserPortfolioActorService" ServicePackageActivationMode="ExclusiveProcess">
    <StatefulService ServiceTypeName="UserPortfolioActorServiceType" TargetReplicaSetSize="3" MinReplicaSetSize="3">
      <UniformInt64Partition PartitionCount="1" LowKey="-9223372036854775808" HighKey="9223372036854775807" />
    </StatefulService>
  </Service>
</DefaultServices>

-----------------



*** PRESENTATION_CORE ***
----------------------------

Front Page
Agenda
High-level overview
User demo
Bot demo
Services
Architecture

*foreach Service [10]
	Specification
	Demo snippet
	SFX snippet
	Code snippet

*foreach SF specific feature [6]
	Specification
	SFX snippet
	Code snippet

Q&A
End page

----------------------------

1)	[Stateless]			Aggregator
2)	[Stateless]			Authentication
3)	[Stateless]			LeaderboardStateless
4)	[API Stateless]		AngularClient
5)	[API Stateless]		PlatformAPIV2
6)	[API Stateless]		TRealTimeBroadcaster
7)	[Stateful]			TradingStateful
8)	[Stateful]			UserProfile
9)	[Stateful Actor]	UserPortfolioActor
0)	[Guest Exe]			PythonBot

----------------------------

0)	Failover manager
1)	Failover of failover manager
2)	Resource governance & custom metrics
3)	Affinity & placement constraints
4)	Auto-scaling & overload
5)	Upgrade, rollback & service versioning
6)	Reverse proxy
7)	Local deployment via SF cmdlets
8)	Cloud deployment & chaos testing

----------------------------

Aggregator
	Interface for RPC
	Implementation of interface
	Proxy creation
	Service instance listener

Authentication

Leaderboard

AngularClient
	Service manifest
	WWWRoot
	Service instance listener

PlatformAPI
	Controllers
	SwaggerUI
	SpecificOrigin
	
RealTimeBroadcaster
	Service manifest
	Special proxy creation

Trading
	Service replica listener
	Retry logic idea of workflow
	Transaction usage for consistency

UserProfile
	
UserPortfolio
	Explanation
	No need for transaction usage

PythonBot
	Script snippet
	Commands to get it as guest exe
	
-----------------------

Failover manager
	Turn off stateful service

Failover of failover manager
	Turn off failover manager

Resource governance & custom metrics
Auto-scaling & overload
	Uncomment the code
	Perform a few registers
	Perform a few logins
	Comment the code
	Additional config setting for faster reporting

Affinity & placement constraints
	Uncomment the code
	Run SF until there is basis for affinity
	Comment the code

Upgrade, rollback & service versioning
Local deployment via SF cmdlets
	Provide the commands [start, upgrade, rollback, removal]
	Default services
	Deploy app via commands
	Change something
	Wait for app to upgrade
	Possibly roll it back

Reverse proxy
	Just the endpoint from Angular
	Explain what it does
	Slower first request

Cloud deployment & chaos testing

-----------------------