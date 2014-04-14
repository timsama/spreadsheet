
#ifndef SPREADSHEET_H
#define SPREADSHEET_H

#include "dependency_graph.h"
#include <string>
#include <set>
#include <queue>
#include <sqlite3.h>

namespace sss {

  enum command_type {
    ENTER, UNDO
  };

  struct command {
    int fromSocket;
    command_type command;
    std::string version;
    std::string cell;
    std::string contents;
  };

  class spreadsheet {
    
  private:
    // Data
    std::string name;
    int version;
    std::map<std::string, std::string> cells;
    sss::dependency_graph dependencies;
    sqlite3 *db;

    // Utilities
    bool free_from_circular(std::string cell, std::set<std::string> content_cells);
    std::set<std::string> get_cells(std::string contents);
    
  public:
  // Constructor / Destructor
    spreadsheet(sqlite3 *db);
    ~spreadsheet();

  // Accessors
    
    // 

  // Modifiers
    
    // Returns 0 if circular dependency
    //  or >0 new version number
    int enter(std::string cell, std::string contents);

    // Returns 0 if unable to undo
    //  or >0 new version number
    int undo();

  };

}

#endif
