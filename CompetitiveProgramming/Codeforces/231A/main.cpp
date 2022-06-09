#include <bits/stdc++.h>

using namespace std;

int main() {
    int n; // number of problems in the contest
    cin >> n;

    int a, b, c;
    int numberOfSubmittedContests = 0;

    for(int i=0; i<n; i++){
        cin>>a>>b>>c;
        numberOfSubmittedContests += ((a+b+c > 1) ? 1 : 0);
    }

    cout<<numberOfSubmittedContests;
}