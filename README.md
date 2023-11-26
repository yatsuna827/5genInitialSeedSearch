# 5genInitialSeedSearch
ポケモンBW1/2の固定乱数の高速なseed検索の実装です。
SIMDと並列計算、その他細かい定数倍最適化を施してあります。

[5genInitialSeedSearch](./5genInitialSeedSearch/) は古い実装で、あまり速くないです。

[MTSeedSearcher](./MTSeedSearcher/)は個体値からMTの初期seedを検索するプログラムです。
[StartingDateTimeSearcher]はMTの初期seedから起動時刻を検索するプログラムです。
