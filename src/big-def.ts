///<reference path="big.js"/>

declare var Big: any;

interface IBig {
    toFixed(precision: number): string;
    lt(t: IBig): boolean;
    lte(t: IBig): boolean;
    eq(t: IBig): boolean;
    gt(t: IBig): boolean;
    gte(t: IBig): boolean;
    abs(): IBig;
    cmp(b: IBig): number;
    div(y: IBig): IBig;
    mul(y: IBig): IBig;
    minus(o: IBig): IBig;
    plus(o: IBig): IBig;
    mod(o: IBig): IBig;
}

class BigFactory {
    public static create(bigDecimal: number | string = 0.0): IBig
    {
        return new Big(bigDecimal) as IBig;
    }
}