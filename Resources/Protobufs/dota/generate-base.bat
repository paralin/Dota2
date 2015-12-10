@echo off

rem we use dota as the GC message base since it's the most updated

echo Building GC base
..\..\Protogen\protogen -s:..\ -i:"steammessages.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\SteamMsgBase.cs" -t:csharp -ns:"Dota2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"gcsystemmsgs.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\SteamMsgGCSystem.cs" -t:csharp -ns:"Dota2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"base_gcmessages.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\SteamMsgGC.cs" -t:csharp -ns:"Dota2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"gcsdk_gcmessages.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\SteamMsgGCSDK.cs" -t:csharp -ns:"Dota2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"econ_gcmessages.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\SteamMsgGCEcon.cs" -t:csharp -ns:"Dota2.GC.Internal"

echo Building Steam datagram 
..\..\Protogen\protogen -s:..\ -i:"steamdatagram_messages.proto" -o:"..\..\..\src\Dota2\Base\Generated\Datagram\SteamMsgDatagram.cs" -t:csharp -ns:"Dota2.Datagram.Internal"

echo Building Dota messages...
..\..\Protogen\protogen -s:..\ -i:"network_connection.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\NetworkConnection.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"

:: dota messages reference some types from the gc base, so we need to import the reference for it
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_common.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCCommon.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_client.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCClient.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_server.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCServer.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_client_fantasy.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCClientFantasy.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"networkbasetypes.proto" -o:"..\..\..\src\Dota2\Base\Generated\Networking\MsgNetworkTypes.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"netmessages.proto" -o:"..\..\..\src\Dota2\Base\Generated\Networking\MsgNetMessages.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"ai_activity.proto" -o:"..\..\..\src\Dota2\Base\Generated\Networking\AiActivity.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"dota_usermessages.proto" -o:"..\..\..\src\Dota2\Base\Generated\Networking\DotaUserMessages.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"usermessages.proto" -o:"..\..\..\src\Dota2\Base\Generated\Networking\UserMessages.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"dota_commonmessages.proto" -o:"..\..\..\src\Dota2\Base\Generated\Networking\MsgDotaCommonMessages.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="SteamKit2.GC.Internal"