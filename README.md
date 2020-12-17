# TaskBud
Simple and  Lightweight Task Organizer/Manager Web Server, written in Asp.Net MVC Core

Demo of the app in action: https://www.twitch.tv/videos/647569153

<img src="Resources/IndexPage.jpg"> <img src="Resources/HistoryPage.jpg">

# Features and Functionality

* Supports Markdown in Task Descriptions
* Mobile first design, just swipe to mark tasks as done and to claim them!
* Minimalist, slick interface
* Invitation Code based Registration, with simple copy and paste invitation links
* Currently supported Databases: PostgreSQL, MSSQL (More to come hopefully!)
* Easy configuration via AppSettings.json / command line args
* Open Source, free to use!
* Built with .NET Core, which means full cross platform support, host it on whatever you please!
* Web Server means everything is served over browsers, so its platform agnostic!
* Designed utilizing SignalR for Real-Time app updates, no need to refresh the page, tasks auto-pop up when updated/created, and auto-dissapear when claimed/completed by any user!
* Simple History interface to give basic "Undo" and "Redo" functionality

Work in progress before first release!

# Setup Guide
## All Platforms
Step 1: Download your matching release zip from the latest release link here: https://github.com/SteffenBlake/TaskBud/releases/latest

Step 2: Unzip the archive

Step 2: Have an empty, ready to use Database of the supported types (Listed in the AppSettings.json file)

Step 3: Modify the `ConnectionType` property to be one of the values supported (listed directly above), matching your DB type

Step 4: Modify the `ConnectionString` value to point to your database. 
Guide on connection strings here: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-strings

Step 5: Modify the `Urls` value to specify what urls to accept requests from.

Step 6 (Option if you want to expose TaskBud for online, non-local use): Make sure to setup your firewalls and etc to expose the port(s) you are listening on, and configure your router as well. 

## Windows
Step 7: Simply run `TaskBud.Website.exe` as Administrator and everything should be ready to go. The command line should inform you its now listening on your URLS.


## Linux/OSX/Etc

Step 7: Install the latest recommended .Net Core runtime: https://dotnet.microsoft.com/download/dotnet-core

Step 8: Navigate your commandline to the primary folder (The one that has all the .dll files in it, and the AppSettings.json file)

Step 9: run the command `dotnet run TaskBud.Website.dll`

## All Platforms

Step 10: Now open up your browser and navigate the one of the URLs you set to listen on, if all went well your website should boot up!

Step 11: Login as Administrator, default Username/Pass is Admin/Admin.

Step 12: **Please ensure you change Administrator default password!** This can be changed via `Menu > Profile - Admin > Password`

Thats all!

# ToDo List
- [ ] Make Invitation Code screen more mobile friendly
- [ ] Enforce Administrator password change, or at the very least bug you about it
- [x] History+Undo Functionality
- [ ] Support more database types
- [ ] Add support for opt-in 2FA, with configurable email server bits
- [ ] Add support for Azure Web Service hosting with Azure Key Vault Storage
- [ ] Maybe add support for attaching images to issues? If people really want that.
- [ ] Add support for push notifications while open, opt in(?)
- [ ] Add configuration and support for SSL Signing / HTTPS
- [x] Add a "Wait until" property to Tasks
- [x] Add a "Automatic Repeat" functionality to Tasks
- [ ] Expose usable API for IoT stuff / IFTTT / Home Assistants
- [ ] Setup wiki and document better
- [ ] Make some "how to use" youtube videos
...
- [x] Acknowledge the irony of a To-Do list on the README for your To-Do list app...