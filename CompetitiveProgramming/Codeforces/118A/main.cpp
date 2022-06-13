#include <bits/stdc++.h>
#include <cctype>

using namespace std;

int main() {
    string input;
    string output;
    cin>>input;
    
    vector<char> vowels = {'A', 'O', 'Y', 'E', 'U', 'I'};

    for(int i=0;i<input.size();i++) {
        if(find(vowels.begin(), vowels.end(), toupper(input[i])) != vowels.end()) {
            continue;
        }

        output.push_back('.'); // insert . before each consonant
        output.push_back(isupper(input[i]) ? tolower(input[i]) : input[i]);
    }

    cout<<output;
}