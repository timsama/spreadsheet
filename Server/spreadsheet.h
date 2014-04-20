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
    std::map<std::string, std::string> get_cells();

  // Modifiers
    
    // Returns 0 if circular dependency
    //  or >0 new version number
    int enter(std::string cell, std::string contents);

    // Returns 0 if unable to undo
    //  or >0 new version number
    //  has out parameters to retrieve new state:
    //   either the cell and contents
    //   or blank strings if nothing to undo 
    int undo(std::string *cell, std::string *contents);

    // Utilities
    bool free_from_circular(std::string cell, std::string contents);

  };

}

#endif
