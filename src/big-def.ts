///<reference path="big.js"/>

declare var Big: any;

interface IBig {
    toFixed(precision: number): string;
    lt(target: IBig): boolean;
}

class BigFactory {
    public static create(bigDecimal: number | string = 0.0): IBig
    {
        return new Big(bigDecimal) as IBig;
    }
}