#include <iostream>
#include <sqlite3.h>
#include <stdint.h>
#include <cstring>

using namespace std;

int64_t spreadsheet_enter(sqlite3 *db, string spreadsheet, string cell, string contents);

int main (int argc, const char* argv[]) {

  sqlite3 *db;
  
  int error = sqlite3_open("test.db", &db);

  // Turn on extended error codes
  sqlite3_extended_result_codes(db, 1);

  if(error) {
    cout << "Can't open database: " << sqlite3_errmsg(db) << endl;
    return 0;
  } else {
    cout << "Opened database successfully" << endl;
    int r = spreadsheet_enter(db, "test", "A1", "Hello");
    cout << "Added row " << r << endl;
  }
  
  sqlite3_close(db);

}

int64_t spreadsheet_enter(sqlite3 *db, string spreadsheet, string cell, string contents) {
  if(!db) return 0;

  char *error = 0;
  sqlite3_stmt *statement;
  const char *leftover;  // Used to point to the next statement if there is one
  char *sql;
  int rc;
  
  sql = (char *)"INSERT INTO transactions (spreadsheet, cell, contents) VALUES (?,?,?)";

  rc = sqlite3_prepare(db, sql, strlen(sql), &statement, &leftover);

  if( rc == SQLITE_OK ) {
    // Bind values
    sqlite3_bind_text(statement, 1, spreadsheet.c_str(), strlen(spreadsheet.c_str()), 0);
    sqlite3_bind_text(statement, 2, cell.c_str(), strlen(cell.c_str()), 0);
    sqlite3_bind_text(statement, 3, contents.c_str(), strlen(contents.c_str()), 0);

    // Commit
    sqlite3_step(statement);
    sqlite3_finalize(statement);

    // Return new rowid
    return sqlite3_last_insert_rowid(db);
  } else {
    cout << "There was an error preparing the statement: " << rc << endl;
    return 0;
  } 
    
}
