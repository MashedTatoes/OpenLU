﻿

namespace OpenLU.Auth

open RakDotNet
open OpenLU.Core
open RakDotNet.RakNet
open System.Net
open System
open OpenLU.Services

open OpenLU.Core
open System.Diagnostics

type AuthServer() as this = 
    inherit LUServer(1001,"3.25 ND1","Auth Server")
    do
        this.HandlerMap.Add((uint64)83,new HandshakeEvent(this.Handshake))
        this.HandlerMap.Add((uint64)339,new LoginEvent(this.ClientLogin))
    interface IAuthServerService with
        member this.Start() = this.StartServer()

    member this.Handshake (ipep : IPEndPoint) (packet : LUPacket) = 
        Console.WriteLine("Handshake")
        
        let response = BitStream()
        let client_version = packet.Body.ReadUInt32()
        response.WriteUInt64(packet.Header)
        response.WriteUInt32(client_version)
        response.WriteUInt32((uint32)0x93)
        response.WriteUInt32((uint32)1)
        let currentProcess = Process.GetCurrentProcess()
        response.WriteUInt32((uint32)currentProcess.Id)
        response.WriteString("127.0.0.1",33)
        this.Server.Send(response,ipep) |> ignore
    member this.ClientLogin(ipep : IPEndPoint) (packet : LUPacket) =
        let username = packet.Body.ReadString(33,true)
        let pwd = packet.Body.ReadString(41,true)
        Console.WriteLine("Login with Username: {0} and Password {1}",username,pwd)
        let response = BitStream()
        response.WriteInt32(0x00000553)
        response.WriteInt32(0)
        
        response.WriteUInt8((uint8)0x01)
        response.WriteString("Talk_Like_A_Pirate")
        response.WriteString("",33*7)
        response.WriteShort((int16)1)
        response.WriteShort((int16)10)
        response.WriteShort((int16)64)
        let userkey = String.Concat(Guid.NewGuid().ToString().ToCharArray(0,20))
        Console.WriteLine("New user of : {0}",userkey)
        response.WriteString(userkey,wide = true)
        response.WriteString("127.0.0.1")
        response.WriteString("127.0.0.1")
        response.WriteUInt16((uint16)2002)
        response.WriteUShort((uint16)3003)
        response.WriteString("0",33)
        response.WriteString("00000000-0000-0000-0000-000000000000",37)
        response.WriteInt32(0)
        response.WriteString("US",3)
        response.WriteByte((byte)0)
        response.WriteByte((byte)0)
        response.WriteULong((uint64)0)
        
        response.WriteString("Hello there ;D",wide = true)

        
        response.WriteInt((int32)4)
        this.Server.Send(response,ipep)




        
    override this.HandlePacket ipep data =
        
        let luPacket = LUPacket(data)
        Console.WriteLine("Header: {0}",luPacket.Header)
        this.HandlerMap.[luPacket.Header].DynamicInvoke(ipep,luPacket) |> ignore
        Console.WriteLine("received packet from {0}:{1} with id {2}",ipep.Address,ipep.Port,data.[0]);
    

    
