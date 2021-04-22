# About Alibi:
Alibi is an implementation of the [Ace Attorney Online 2](https://aceattorneyonline.com) protocol (Which can be found [here](https://github.com/AttorneyOnline/docs/blob/master/docs/development/network.md)) that focuses on:

1. Safety of the server and it's players
2. Security of the server owner
3. Stability at all points in the chain
4. Being predictable, deterministic, and easy to maintain

It achieves this in a few ways. Firstly, Alibi will only accept packets from a client that is correctly following protocol. For example, say a client opens a connection, and immediately sends an IC message. That client will be kicked from the server for disobeying protocol. This is done to heavily discourage spambots, minimize exploits, and who knows, maybe it'll encourage client developers to make better clients :glare:

Alibi also is very easy to maintain because everything is designed to be as plug n' play as possible. Configuration is automatically generated with "the best" defaults, just by starting the server, plugins are single, small DLLs dropped into a folder, accessing logs is as simple as checking the dumped logged if the server crashes, or dumping the log buffer while it's still running. Modified configuration can be hot-loaded while the server is running to allow server owners to make changes, prevent spam, and respond to community requests quickly and without interrupting the game.

# Contributing

It's also worth noting that I am **always** open to issues, pull requests, debugging specific issues in private, whatever. I could really use it, actually. My Discord is ElijahZAwesome#6933.