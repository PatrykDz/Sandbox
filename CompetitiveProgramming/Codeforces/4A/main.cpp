#include <bits/stdc++.h>

using namespace std;

int main() {
    int input;
    cin >> input;

    string canDivide = (input > 2 && input % 2 == 0) ? "YES" : "NO";
    cout << canDivide;
}