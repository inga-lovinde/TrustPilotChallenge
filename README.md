Info
====

This is my solution to the challenge: http://followthewhiterabbit.trustpilot.com/

Usage info
==========

```
WhiteRabbit.exe < wordlist
```

Performance
===========

Memory usage is minimal (for that kind of task), around 10-30MB.

It is also somewhat optimized for likely intended phrases, as anagrams consisting of longer words are generated first.
That's why the given hashes are solved much sooner than it takes to check all anagrams.

Anagrams generation is not parallelized, as even single-threaded performance for 4-word anagrams is high enough; and 5-word (or larger) anagrams are frequent enough for most of the time being spent on computing hashes, with full CPU load.

Multi-threaded performance with RyuJIT (.NET 4.6, 64-bit system) on quad-core Sandy Bridge @2.8GHz is as follows:

* If only phrases of at most 4 words are allowed, then it takes less than 5.5 seconds to find and check all 7433016 anagrams; all hashes are solved in first 0.7 seconds.

* If phrases of 5 words are allowed as well, then it takes around 17 minutes to find and check all anagrams; all hashes are solved in first 25 seconds. Most of time is spent on MD5 computations for correct anagrams, so there is not a lot to optimize further.

* If phrases of 6 words are allowed as well, then "more difficult" hash is solved in 30 seconds, "easiest" in 3 minutes, and "hard" in 6 minutes.

* If phrases of 7 words are allowed as well, then "more difficult" hash is solved in 6 minutes.

Note that all measurements were done on a Release build; Debug build is significantly slower.

For comparison, certain other solutions available on GitHub seem to require 3 hours to find all 3-word anagrams. This solution is faster by 5-7 orders of magnitude (it finds and checks all 4-word anagrams in 1/2000th fraction of time required for other solution just to find all 3-word anagrams, with no MD5 calculations).
