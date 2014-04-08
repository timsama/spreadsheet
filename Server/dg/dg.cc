/*
 * A class to model a set of ordered pairs of strings
 */

#include "dg.h"

dependency_node::dependency_node(std::string n) { }

dependency_graph::dependency_graph () { 
  // size starts at 0
  dependency_graph::size_of_node_set = 0;
}

dependency_graph::~dependency_graph () {

}

int dependency_graph::size() {
  return this->size_of_node_set;
}

const int dependency_graph::operator[] (const std::string s) const { 
  std::map<std::string, dependency_node>::const_iterator it; 
  it = this->node_set.find(s);

  if(it != this->node_set.end()) {
    return it->second.dependees.size();
  } else {
    return 0;
  }
  
}

bool dependency_graph::has_dependents (std::string s) { 
  // Find the node
  std::map<std::string, dependency_node>::const_iterator it; 
  it = this->node_set.find(s);

  // return true if the node has dependents
  if(it != this->node_set.end()) {
    return (it->second.dependents.size() > 0);
  } else {
    return false;
  }
}

bool dependency_graph::has_dependees (std::string s) { 
  // Find the node
  std::map<std::string, dependency_node>::const_iterator it; 
  it = this->node_set.find(s);

  // return true if the node has dependees
  if(it != this->node_set.end()) {
    return (it->second.dependees.size() > 0);
  } else {
    return false;
  }
}

std::set<std::string> dependency_graph::get_dependents (std::string s) { 
  // Find the node
  std::map<std::string, dependency_node>::const_iterator it; 
  it = this->node_set.find(s);

  if(it != this->node_set.end()) {
    return it->second.dependents;
  } else {
    std::set<std::string> empty;
    return empty;
  }
}

std::set<std::string> dependency_graph::get_dependees  (std::string s) { 
  // Find the node
  std::map<std::string, dependency_node>::const_iterator it; 
  it = this->node_set.find(s);

  if(it != this->node_set.end()) {
    return it->second.dependees;
  } else {
    std::set<std::string> empty;
    return empty;
  }
}

void dependency_graph::add_dependency (std::string s, std::string t) { 
  // establish a node s
  if(this->node_set.count(s) != 1) {
    this->node_set.insert(std::pair<std::string, dependency_node>(s, dependency_node(s)));
  }
  // establish a node t
  if(this->node_set.count(t) != 1) {
    this->node_set.insert(std::pair<std::string, dependency_node>(t, dependency_node(t)));
  }

  std::map<std::string, dependency_node>::iterator it;

  // get node s and add t
  it = this->node_set.find(s);
  if(it->second.dependents.count(t) != 1) {
    it->second.dependents.insert(t);
    this->size_of_node_set++; // Added
  }

  // get node t and add s
  it = this->node_set.find(t);
  if(it->second.dependees.count(s) != 1) {
    it->second.dependees.insert(s);
  }
}

void dependency_graph::remove_dependency (std::string s, std::string t) {

  std::map<std::string, dependency_node>::iterator it;

  // get node s and add t
  it = this->node_set.find(s);
  if(it != this->node_set.end() && it->second.dependents.count(t) == 1) {
    it->second.dependents.erase(t);
    this->size_of_node_set--; // Found and removed
  }

  // get node t and add s
  it = this->node_set.find(t);
  if(it != this->node_set.end() && it->second.dependees.count(s) == 1) {
    it->second.dependees.erase(s);
  }

}

void dependency_graph::replace_dependents (std::string s, std::set<std::string> new_dependents) { 

  std::map<std::string, dependency_node>::iterator node;

  // find s
  node = this->node_set.find(s);

  // if node s exists
  if(node != this->node_set.end()) {
    // remove old dependent pairs
    for(std::set<std::string>::iterator it = node->second.dependents.begin(); it != node->second.dependents.end(); ++it) {
      this->remove_dependency(s, *it);
    }    
  }
  
  // add new dependent pairs
  for(std::set<std::string>::iterator it = new_dependents.begin(); it != new_dependents.end(); ++it) {
    this->add_dependency(s, *it);
  }

}

void dependency_graph::replace_dependees (std::string s, std::set<std::string> new_dependees) { 

  std::map<std::string, dependency_node>::iterator node;

  // find s
  node = this->node_set.find(s);

  // if node s exists
  if(node != this->node_set.end()) {
    // remove old dependees
    for(std::set<std::string>::iterator it = node->second.dependees.begin(); it != node->second.dependees.end(); ++it) {
      this->remove_dependency(*it, s);
    }    
  }
  
  // add new dependees
  for(std::set<std::string>::iterator it = new_dependees.begin(); it != new_dependees.end(); ++it) {
    this->add_dependency(*it, s);
  }

}
