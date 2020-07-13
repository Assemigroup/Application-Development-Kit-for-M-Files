# Contributing
Thank you for considering contributing. We believe that the open source community will drive this to even greater heights. By contributing you agree to abide by our [code of conduct](CODE_OF_CONDUCT.md).
## 0. What contributions do you accept?
This repository is built on top of the APIs and Frameworks of [M-Files](http://www.m-files.com) with the goal of providing a higher level of access / abstraction to the M-Files API for simplified and expedited development. At its core, the abstraction layer adheres to these basic philosophies.
* Represent the Vault Metadata Structure as specific Objects, Classes, Property Definitions and Value Lists in code.
* Provide methods to operate on these objects directly.
* Provide additional methods and extensions to simplify/extend M-Files API and Framework’s native vault functions.

We welcome any forms of contribution that adhere to these goals.
At a minimum, all contributions must be:
* **Consistent in style and format** to other code.
* **Self-documenting or commented and organized**. This is a repository for developers to learn. If your code is not easy to understand then we may ask you to refactor it.
* **Useful to, and usable by**, other developers. If your sample or code change has limited scope or breaking changes then we may ask you to maintain your fork separately, and we may point developers to it as necessary.

## How do I contribute?
### 1. Research
If you have identified a bug, or an area in which the code could be improved, then [search the issue tracker](https://github.com/AssemiGroup/Application-Development-Kit-for-M-Files/issues) to see if someone else in the community has already created a ticket. If not, go ahead and make [one](https://github.com/AssemiGroup/Application-Development-Kit-for-M-Files/issues/new)!
Please ensure that:
* Bugs should have the phrase `BUG:` at the start of the title. A good title would be `BUG: method XXXX does not correctly set property YYYY to AAAA`.
* A request for an additional sample should have `SAMPLE:` at the start of the title. A good title would be `SAMPLE: Please add an example of how to use method XXXX`.
* A request for an additional feature should have `FEATURE:` at the start of the title. A good title for a new feature would be `FEATURE: XXXX new faster object search method`. Please fully document how you expect this method to function.

### 2. Fork & create a branch
If this is something you think you can fix, then [fork the repository](https://help.github.com/articles/fork-a-repo) and create a branch with a descriptive name `(bugs = bugfix/; features = feature/; hotfix = hotfix/)`.
A good branch name would be (where feature issue #88 is the ticket you're working on):

```sh
git checkout -b feature/88-new-faster-object-search-method 
```
### 3. Did you find a bug?
* **Ensure the bug was not already reported** by [searching all issues](https://github.com/AssemiGroup/Application-Development-Kit-for-M-Files/issues).
* If you're unable to find an open issue addressing the problem, open a new one. Be sure to include a **title and clear description**, as much relevant information as possible. Include:
    * Steps that can be taken to reproduce the issue.
    * M-Files server and versions.
    * Operating systems affected.
    * If possible, a **code sample** or an **executable test case** demonstrating the expected behavior that is not occurring.
### 4. Implement your fix or feature
At this point, you're ready to make your changes within your new branch!
Confirm that your changes perform as expected. Fully document any performance, security or version requirements.
### 5. Make a pull request
At this point, you should switch back to your master branch and make sure it's up to date with master:
```sh
git remote add upstream git@github.com: Assemigroup/Application-Development-Kit-for-M-Files.git
git checkout master
git pull upstream master
```
Then update your feature branch from your local copy of master, and push it!
```sh
git checkout feature/88-new-faster-object-search-method
git rebase master
git push --set-upstream origin feature/88-new-faster-object-search-method
```
Finally, go to GitHub and [make a pull request](https://help.github.com/articles/creating-a-pull-request).
### 6. Keeping your pull request updated
If a maintainer asks you to "rebase" your pull request, they're saying that a lot of code has changed since you forked your code, and that you need to update your branch so it's easier to merge.
To learn more about rebasing in Git, there are a lot of [good](http://git-scm.com/book/en/Git-Branching-Rebasing) [resources](https://help.github.com/articles/interactive-rebase), but here's the suggested workflow:
```sh
git checkout feature/88-new-faster-object-search-method
git pull --rebase upstream master
git push --force-with-lease feature/88-new-faster-object-search-method

```
**Once your code is rebased, you will need to re-run any tests**.
## Guidelines for merging a pull request
A pull request can only be merged by a maintainer if:
* It is up to date with current master.
* It passes our internal test and review.
* It passes our other contributing guidelines.

