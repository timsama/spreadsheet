server: run_server.cc Server.cc Server.h SS_Server.o MessageHandler.o Serv_Sock.o dependency_graph.o spreadsheet.o spreadsheet_db.o parsecells.o
	g++ -I/usr/local/stow/boost/boost_1_53_0/include -L/usr/local/stow/boost/boost_1_53_0/lib -o server run_server.cc Server.cc SS_Server.o MessageHandler.o Serv_Sock.o dependency_graph.o parsecells.o spreadsheet.o spreadsheet_db.o -lsqlite3 -lboost_thread -pthread

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

sstest:			spreadsheet_test.cc spreadsheet.o dependency_graph.o spreadsheet_db.o parsecells.o
			g++ -o sstest spreadsheet_test.cc spreadsheet.o dependency_graph.o spreadsheet_db.o parsecells.o -lsqlite3

spreadsheet.o:		spreadsheet.cc spreadsheet.h parsecells.o
			g++ -c spreadsheet.cc parsecells.o

parsecells.o:		parsecells.cc parsecells.h
			g++ -c parsecells.cc

dgtest:			dependency_graph_test.cc dependency_graph.o
			g++ -o dgtest dependency_graph_test.cc dependency_graph.o

dependency_graph.o:	dependency_graph.cc dependency_graph.h
			g++ -c dependency_graph.cc

clean:	
			rm -f a.out *.o *.*~ dgtest sstest sqltest server
