Info
====

This is my solution to the challenge: http://followthewhiterabbit.trustpilot.com/

The task is to find anagrams of the phrase "**poultry outwits ants**" with the required MD5 hashes, using the supplied dictionary:

```
e4820b45d2277f3844eac66c903e84be # easy
23170acc097c24edb98fc5488ab033fe # more difficult
665e5bcb0c20062fe8abaaf4628bb154 # hard
```

And some more hashes for you to do:

```
e8a2cbb6206fc937082bb92e4ed9cd3d
74a613b8c64fb216dc22d4f2bd4965f4
ccb5ed231ba04d750c963668391d1e61
d864ae0e66c89cb78345967cb2f3ab6b
2b56477105d91076030e877c94dd9776
732442feac8b5013e16a776486ac5447
```

Usage info
==========

```
WhiteRabbit.exe < wordlist
```

**Note that this code only works correctly on little-endian x64 systems, due to heavy optimizations of MD5 computation!**

Performance
===========

Memory usage is minimal (for that kind of task), less than 10MB (25MB for MaxNumberOfWords = 8).

It is also somewhat optimized for likely intended phrases, as anagrams consisting of longer words are generated first.
That's why the given hashes are solved much sooner than it takes to check all anagrams.

Anagrams generation is not parallelized, as even single-threaded performance for 4-word anagrams is high enough; and 5-word (or larger) anagrams are frequent enough for most of the time being spent on computing hashes, with full CPU load.

Multi-threaded performance with RyuJIT (.NET 4.6, 64-bit system) on i5-6500 is as follows (excluding initialization time of 0.2 seconds), for different maximum allowed words in an anagram:

Number of words|Time to check all anagrams no longer than that|Time to solve "easy" hash|Time to solve "more difficult" hash|Time to solve "hard" hash|Number of unique anagrams no longer than that
---------------|----------------------------------------------|-------------------------|-----------------------------------|-------------------------|---------------------------------------------
3|0.04s||||4560
4|0.45s|||0.08s|7,431,984
5|18s|0.25s|0.07s|0.5s|1,347,437,484
6|9.5 minutes|1.7s|0.3s|4.3s|58,405,904,844
7|3.2 hours|10s|1.3s|28s|1,070,307,744,114
8|49 hours|37s|3.8s|1.9 minutes|10,893,594,396,594
9||2.5 minutes|13s|9.5 minutes|70,596,864,409,954
10||5 minutes|21s|17.5 minutes|314,972,701,475,754

Note that all measurements were done on a Release build; Debug build is significantly slower.

For comparison, certain other solutions available on GitHub seem to require 3 hours to find all 3-word anagrams. This solution is faster by 6-7 orders of magnitude (it finds and checks all 4-word anagrams in 1/10000th fraction of time required for other solution just to find all 3-word anagrams, with no MD5 calculations).

Conditional compilation symbols
===============================

* Define `DEBUG`, or build in debug mode, to get the total number of anagrams (not optimized).

Implementation notes
====================

1. We need to limit the number of words in an anagram by some reasonable number, as there are single-letter words in dictionary, and computing MD5 hashes for all anagrams consisting of single-letter words is computationally infeasible and could not have been intended by the challenge authors.
In particular, as there are single-letter words for every letter in the original phrase, there are obvious anagrams consisting exclusively of the single-letter words; and the number of such anagrams equals to the number of all letter permutations of the original phrase, which is too high.

2. Every word or phrase could be thought of as a vector in 26-dimensional space, with every component equal to the number of corresponding letters in the original word.
That way, vector corresponding to some phrase equals to the sum of vectors of its words.
We can reduce the problem of finding anagrams (words which add up to a phrase containing the same letters in the same quantity as the original phrase) to the problem of finding sequences of vectors which add up to the vector corresponding to the original phrase.
Of course, several words could be represented by a single vector.
So the first step is: convert words to vectors; find all sequences of vectors which add up to the required sum; convert sequences of vectors back to the sequences of words (with every sequence of vectors potentially generating many sequences of words).

3. Of course, we can ignore all words that contain any letter besides that contained in the original phrase, or that contain too many copies of some letter.
Basically, we only need to consider words which are represented by vectors with all components not greater than that of the vector corresponding to the original phrase.

4. Vector ariphmetic could be done manually, but all modern processors have SIMD support of some sort, which allows for fast vector operations (addition, comparison etc).
It seems that modern instruction set allows one to work with 128-bit vectors; and System.Numerics.Vectors allows us to tap on this feature by offering vectors with byte components in 16-dimensional space.
As the original phrase only contains 12 different characters, it's more than enough for us.

5. Any permutation of the words gives us another anagram; any permutation of vectors does not change their sum.
So we can only consider the sequences of vectors which go in the order specified in the original dictionary (that is, numbers of their positions go in the ascending order), and then consider all permutations of sequences that have the required sum.
As sequences having the required sum are quite rare, that will give us a speedup with the factor of n!, where n is the allowed number of vectors (see note 1).

6. So far, the generation of sequences of vectors is quite simple.
We recursively go through the dictionary, starting with the position of previous word, and checking if all the vectors so far add up to the target sum, until maximum allowed number of vectors is reached.
One obvious optimization is: if some component of the partial sum is larger than the corresponding component of the target, there is no need to process this partial sequence further.

7. Next question is, should we reorder the original dictionary?
It is quite expected that, if longer (in a certain sense) words go first, we'll have less possible variants to check, as we'll reach the partial sum that could be discarded (see note 6) sooner.
It turns out that we can get pretty noticeable speedup this way: total number of processed sequences goes down from 62 millions to 29 millions in a three-word case, and from 1468 millions to 311 millions in a four-word case.
The ordering we use is as follows: every letter gets assigned a weight which is inversely proportional to the number of occurrences in the original phrase.
This way, every component of the original phrase is weighed equally.
Then, words get ordered by weight in a descending order.

8. Note that such a weight acts like a norm on our non-negative pseudospace.
What's more, it is a linear function, meaning that weight of sum of vectors equals sum of weights.
It means that, if we're now checking a vector such that its weight, multiplied by a number of words we're ready to allow in the sequence, is less than the distance between current partial sum and a target, there is no point in checking sequences containing this word (or anything smaller) for this partial sequence.
As we have ordered the words by weight, when we're looping over the dictionary, we can check the weight of the current item, and if it's lower than our threshold, we can just break the loop.

9. Another possible optimization with such an ordering is employing binary search.
There is no need in processing all the words that are too large to be useful at this moment; we can start with a first word with a weight not exceeding distance between current partial sum and the target.

10. And then, all that remains are implementation optimizations: precomputing weights, optimizing memory usage and loops, using byte arrays instead of strings, etc. Some of optimizations which hurt code readability:
    * Words are stored as byte arrays (one byte per character, as we're working with ASCII), with trailing space (to make concatenating words into anagram easier);
    * Anagrams are stored in a way optimized for MD5 - as MD5 message (i.e. with trailing "128" byte, as an array of 8 uints, with last uint set to anagram length * 8). For example, "poultry outwits ants" is stored as fixed 32-byte memory area containing "poultry outwits ants" + 0x80 + (0x00)x7 + (uint)0x50 (for 20 characters).

11. Filtering the original dictionary (e.g. throwing away all single-letter words) does not really improve the performance, thanks to the optimizations mentioned in notes 7-9.
This solution finds all anagrams, including those with single-letter words.

12. Computing the entire MD5, and then comparing it to the target MD5s, makes little sense. Each of MD5 components is `uint`, which means that the chances of first component match for different hashes are one in 4 billions.
It's more efficient to compute only the first component (which is 5% faster since we don't need to perform rounds 62-64 of MD5), and use only the first component for a lookup (which makes the lookup 4x faster).
To prevent false positives, we could compute the entire MD5 again if there is a match.
As that will only happen once in 4 billion hashes, the efficiency of this computation does not matter at all.
Right now, this additional checking is not implemented, which means that once in a minute (if there are 3 target hashes) the program will produce a false positive, which allows one to monitor progress.

13. MD5 computation is further optimized by leveraging CPU extensions.
For example, one could compute MD5 more effectively by using `rotl` instruction to rotate numbers (which is currently done with two bitshifts and one `or` / `xor`).
What's more important, one could compute 4 hashes at once (on a single core) using SSE, 8 hashes at once using AVX2, or 16 hashes at once using AVX512 (AVX lacks enough instructions to make computing hashes feasible).
.NET/RyuJit does not support some of the required intrinsics (`rotl` for plain MD5 implementation, `psrld` and `pslld` for SSE, and similar intrinsics for AVX2).
Although `rotl` support is expected in next release of RyuJIT (see https://github.com/dotnet/coreclr/pull/1830), no support for bitshift SIMD/AVX2 instructions is currently expected (see https://github.com/dotnet/coreclr/issues/3226).
However, one can move MD5 computations to the unmanaged C++ code, where all the intrinsics are available.
To make this work efficiently, I had to store anagrams in chunks of 8 anagrams (so that unmanaged code will receive the chunk and produce 8 hashes).
And to make this efficient, I had to make all permutation counts to divide by 8 by filling in some additional permutation copies.
It slows down processing anagrams of 1, 2, and 3 words (as for every set of word, number of anagrams is increased to 8 from 1, 2 and 6, respectively); however, these are relatively rare for a given phrase and dictionary.

Implementation details
======================

Given all the above, the implementation is as follows:

1. Words from the dictionary are converted into arrays of bytes with a trailing space.

2. The dictionary is filtered from words that could not be a part of anagram (e.g. "b" or "aa"), and from duplicates.

3. Words are converted into vectors, and grouped by vector.

4. Vectors are ordered by their norm, in a descending order.

5. All sequences of non-decreasing vector indices adding up to a target vector are found.

6. For every sequence, a sequence of word arrays corresponging to these vectors is generated.

7. For every sequence of word arrays, all sequences of word combinations are generated (e.g. for [[ab, ba], [cd, dc]], we generate [ab, cd], [ab, dc], [ba, cd], [ba, dc]).

8. For every sequence of words, all permutations are generated (in chunks of 8).

9. For every 8 permuted sequences of words, `uint[64]` message is generated (8 uints = 28 bytes with a trailing `128` byte, plus a length in bits for every sequence).

10. For every `uint[64]` message, 8 `uint`s corresponding to the first components of MD5 hashes for `uint[8]` messages are generated.

11. Every resulting `uint` is checked against the targets; if match is found, both sequence of word and full MD5 hash are printed to the output.

