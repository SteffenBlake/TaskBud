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

ToDo list:
- [ ] Make Invitation Code screen more mobile friendly
- [x] History+Undo Functionality
- [ ] Support more database types
- [ ] Add support for opt-in 2FA, with configurable email server bits
- [ ] Add support for Azure Web Service hosting with Azure Key Vault Storage
- [ ] Maybe add support for attaching images to issues? If people really want that.
- [ ] Add support for push notifications while open, opt in(?)
