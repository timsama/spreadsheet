#include "dependency_graph.h"
#include "spreadsheet_db.h"
#include <string>
#include <set>
#include <queue>
#include <sqlite3.h>

#ifndef SPREADSHEET_H
#define SPREADSHEET_H

namespace sss {

  class spreadsheet {
    
  private:
    // Data
    std::string name;
    dependency_graph dependencies;
    spreadsheet_db ssdb;
    
  public:
  // Constructor / Destructor
    spreadsheet(std::string name);
    ~spreadsheet();

  // Accessors

    int get_version();
    int enter(std::string cell, std::string contents);
    int undo(std::string *cell, std::string *contents);
    std::map<std::string, std::string> get_cells();

    // Utilities
    bool free_from_circular(std::string cell, std::string contents);

  };

}

#endif
