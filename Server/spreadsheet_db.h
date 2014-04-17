#include <iostream>
#include <sqlite3.h>
#include <stdint.h>
#include <cstring>
#include <map>

#ifndef SPREADSHEET_DB_H
#define SPREADSHEET_DB_H

namespace sss {

  class spreadsheet_db {
  
  private:
 
    std::string name;
    int version;

    sqlite3 *db;

    int open_db();
    int init_table();
    int64_t increase_version();

  public:
    spreadsheet_db(std::string name);
    ~spreadsheet_db();

    int64_t undo(std::string *cell_out, std::string *contents_out);
    int64_t enter(std::string cell, std::string contents);

    std::string get_name();
    int64_t get_version();

    std::map<std::string, std::string> get_cells();
  };

}

#endif
