server: run_server.cc Server.cc Server.h SS_Server.o MessageHandler.o Serv_Sock.o dependency_graph.o spreadsheet.o spreadsheet_db.o
	g++ -o server run_server.cc Server.cc SS_Server.o MessageHandler.o Serv_Sock.o dependency_graph.o spreadsheet.o spreadsheet_db.o -lsqlite3

Serv_Sock.o:		Serv_Sock.cc Serv_Sock.h
			g++ -c Serv_Sock.cc

MessageHandler.o:	MessageHandler.cc MessageHandler.h
			g++ -c MessageHandler.cc

SS_Server.o:		SS_Server.cc SS_Server.h
			g++ -c SS_Server.cc

sqltest:		spreadsheet_db_test.cc spreadsheet_db.o
			g++ -o sqltest spreadsheet_db_test.cc spreadsheet_db.o -lsqlite3

spreadsheet_db.o:	spreadsheet_db.h spreadsheet_db.cc 
			g++ -c spreadsheet_db.cc -lsqlite3

sstest:			spreadsheet_test.cc spreadsheet.o dependency_graph.o spreadsheet_db.o
			g++ -o sstest spreadsheet_test.cc spreadsheet.o dependency_graph.o spreadsheet_db.o -lsqlite3

spreadsheet.o:		spreadsheet.cc spreadsheet.h
			g++ -c spreadsheet.cc

dgtest:			dependency_graph_test.cc dependency_graph.o
			g++ -o dgtest dependency_graph_test.cc dependency_graph.o

dependency_graph.o:	dependency_graph.cc dependency_graph.h
			g++ -c dependency_graph.cc

clean:	
			rm -f a.out *.o *.*~ dgtest sstest sqltest server
