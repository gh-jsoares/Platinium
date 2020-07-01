# Platinium

Platinium let's you create any kind of Server-Client based application. All the data handling, connection pipes, and package serialization is done. You just need to create a Plugin and place it at the Server /Plugins folder. 
Inside it provides the IPlugin interface and all libraries for you to create your own plugin.

Want to share the screen with your friend(s)? No problem, just create a library that takes screenshots of the desktop and pack it in a package and it automatically sends it away!
Want to make a simple Chat application? You can do that also! You just need to create the library and load it to the server. Platinium handles all the hard work.

# Planning our design
###### 1. Answer Questions
- What are we designing?
- Who are we designing it for?
- What features do we need to have?

###### 2. User Stories
###### 3. Thinking about the sections

## Questions
	
####	1. What are we designing?
We are designing a GUI for a Windows Application. This application allows the admin to send commands and receive respondes from the client apps.

####	2. Who are we designing it for?
We are designing it for a friend. It will be available for open usage.

####	3. What features do we need to have?
	Send commands to the client
	Receive reponses from the client
	Import and use plugins, that will add new commands
	Kick clients
	View connected clients
	

## User Stories
As a `blank`, I want to be able to `blank`, so that `blank`.

As a _**user**_, I want to be able to ==_send commands do a client and receive a response from it_==, so that ==_the client executes the command and I receive information that it did (or didn't)_==.

As a _**user**_, I want to be able to ==_use/import plugins that I've aquired_==, so that ==_I can use them on the clients_==.

As a _**user**_, I want to be able to ==_view a list of the connected clients_==, so that ==_I know which are online_==.

As a _**user**_, I want to be able to ==_kick clients_==, so that ==_I am able to manage the network_==.
