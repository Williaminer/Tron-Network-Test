# Tron-Network-Test

A small networking test I developed over the Christmas break in 2020 when I was curious as to how multiplayer games are developed.
Focusing on the networking and technical aspect, the graphics are minimal at best, however still rendered using **DirectX** using the **Monogame** framework - which I was also interested in developing in.

Uses custom packets for data sent between clients and servers - is technically UDP peer-to-peer with packet loss, however **Lidgren** mimics a TCP connection with guaranteed delivery, which I used to create a client-server network.

While the code for the program is here it requires a Visual Studio installation to build and run, and to test actual multiplayer functionality _"client.StartClient()"_ within _"GameClient.CS"_ needs to point to the IP address of the peer running the server program, with that IP being port forwarded on port 77.

https://github.com/lidgren/lidgren-network-gen3
https://www.monogame.net/
