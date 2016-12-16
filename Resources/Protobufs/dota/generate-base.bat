@echo off

echo Building Dota GC base...
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"steammessages.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\SteamMsgBase.cs" -t:csharp -ns:"Dota2.GC.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"gcsystemmsgs.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\SteamMsgGCSystem.cs" -t:csharp -ns:"Dota2.GC.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"base_gcmessages.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\SteamMsgGC.cs" -t:csharp -ns:"Dota2.GC.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"gcsdk_gcmessages.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\SteamMsgGCSDK.cs" -t:csharp -ns:"Dota2.GC.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"econ_shared_enums.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\SteamMsgGCEconSharedEnums.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"econ_gcmessages.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\SteamMsgGCEcon.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"

echo Building Dota messages...
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"network_connection.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\NetworkConnection.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"

..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_client_enums.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgClientEnums.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_shared_enums.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgSharedEnums.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_gcmessages_common.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCCommon.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="Dota2.GC.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_gcmessages_common_match_management.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCCommonMatchMgmt.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="Dota2.GC.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_gcmessages_msgid.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCMsgId.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_gcmessages_client.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCClient.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="Dota2.GC.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_gcmessages_client_chat.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCClientChat.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_gcmessages_client_fantasy.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCClientFantasy.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_gcmessages_client_guild.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCClientGuild.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_gcmessages_client_match_management.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCClientMatchMgmt.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal" -p:import="Dota2.GC.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_gcmessages_client_team.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCClientTeam.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_gcmessages_client_tournament.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCClientTournament.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_gcmessages_client_watch.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCClientWatch.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"
..\..\Protogen\protogen -p:lightFramework -s:..\ -i:"dota_gcmessages_server.proto" -o:"..\..\..\src\Dota2\Base\Generated\GC\Dota\MsgGCServer.cs" -t:csharp -ns:"Dota2.GC.Dota.Internal"