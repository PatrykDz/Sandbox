#include <bits/stdc++.h>

using namespace std;

int main() {
  string S;

  cin >> S;

  int n = S.size();
  long long numberOfPairs = 0;

  // for (int i = 0; i < n; i++) {
  //   for (int j = 0; j < n; j++) {
  //     if (S[i] == S[j]) {
  //       nubmerOfPairs++;
  //     }
  //   }
  // }

  map<string, int> dictionary;

  for (int i = 0; i < n; i++) {
    dictionary[to_string(S[i])] += 1;
  }

  map<string, int>::iterator it;
  for (it = dictionary.begin(); it != dictionary.end(); it++) {
    numberOfPairs += pow(it->second, 2);
  }

  cout << numberOfPairs;
}