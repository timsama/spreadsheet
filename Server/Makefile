server: server.cc dependency_graph.o spreadsheet.o
	g++ -o server server.cc dependency_graph.o spreadsheet.o

sstest:	spreadsheet_test.cc spreadsheet.o dependency_graph.o
	g++ -o sstest spreadsheet_test.cc spreadsheet.o dependency_graph.o

spreadsheet.o:	spreadsheet.cc spreadsheet.h
		g++ -c spreadsheet.cc

dgtest:	dependency_graph_test.cc dependency_graph.o
	g++ -o dgtest dependency_graph_test.cc dependency_graph.o

dependency_graph.o:	dependency_graph.cc dependency_graph.h
			g++ -c dependency_graph.cc

clean:	
	rm -f a.out *.o *.*~ dgtest sstest server