/*
 * A class to model a set of ordered pairs of strings
 */

#include <set>
#include <string>
#include <map>
#include <iterator>

#ifndef DEPENDENCY_GRAPH_H
#define DEPENDENCY_GRAPH_H

class dependency_node {

 private:

 public:
  std::string name;
  std::set<std::string> dependents;
  std::set<std::string> dependees;
  dependency_node(std::string n);
};

class dependency_graph {

 private:
  std::map<std::string, dependency_node> node_set;
  int size_of_node_set;

 public:
  dependency_graph ();
  ~dependency_graph ();

  int size();

  const int operator[] (const std::string s) const;

  bool has_dependents (std::string s);
  bool has_dependees (std::string s);

  std::set<std::string> get_dependents (std::string s);
  std::set<std::string> get_dependees  (std::string s);

  void add_dependency (std::string s, std::string t);
  void remove_dependency (std::string s, std::string t);

  void replace_dependents (std::string s, std::set<std::string> new_dependents);
  void replace_dependees (std::string s, std::set<std::string> new_dependees);

};

#endif
