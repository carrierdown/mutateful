// big.js library - patched to work in non-module, non-browser environment
/*
 big.js v3.1.3
 A small, fast, easy-to-use library for arbitrary-precision decimal arithmetic.
 https://github.com/MikeMcl/big.js/
 Copyright (c) 2014 Michael Mclaughlin <M8ch88l@gmail.com>
 MIT Expat Licence
 */
/***************************** EDITABLE DEFAULTS ******************************/
// The default values below must be integers within the stated ranges.
/*
 * The maximum number of decimal places of the results of operations
 * involving division: div and sqrt, and pow with negative exponents.
 */
var DP = 20, // 0 to MAX_DP
/*
 * The rounding mode used when rounding to the above decimal places.
 *
 * 0 Towards zero (i.e. truncate, no rounding).       (ROUND_DOWN)
 * 1 To nearest neighbour. If equidistant, round up.  (ROUND_HALF_UP)
 * 2 To nearest neighbour. If equidistant, to even.   (ROUND_HALF_EVEN)
 * 3 Away from zero.                                  (ROUND_UP)
 */
RM = 1, // 0, 1, 2 or 3
// The maximum value of DP and Big.DP.
MAX_DP = 1E6, // 0 to 1000000
// The maximum magnitude of the exponent argument to the pow method.
MAX_POWER = 1E6, // 1 to 1000000
/*
 * The exponent value at and beneath which toString returns exponential
 * notation.
 * JavaScript's Number type: -7
 * -1000000 is the minimum recommended exponent value of a Big.
 */
E_NEG = -7, // 0 to -1000000
/*
 * The exponent value at and above which toString returns exponential
 * notation.
 * JavaScript's Number type: 21
 * 1000000 is the maximum recommended exponent value of a Big.
 * (This limit is not enforced or checked.)
 */
E_POS = 21, // 0 to 1000000
/******************************************************************************/
// The shared prototype object.
P = {}, isValid = /^-?(\d+(\.\d*)?|\.\d+)(e[+-]?\d+)?$/i, Big;
/*
 * Create and return a Big constructor.
 *
 */
function bigFactory() {
    /*
     * The Big constructor and exported function.
     * Create and return a new instance of a Big number object.
     *
     * n {number|string|Big} A numeric value.
     */
    function Big(n) {
        var x = this;
        // Enable constructor usage without new.
        if (!(x instanceof Big)) {
            return n === void 0 ? bigFactory() : new Big(n);
        }
        // Duplicate.
        if (n instanceof Big) {
            x.s = n.s;
            x.e = n.e;
            x.c = n.c.slice();
        }
        else {
            parse(x, n);
        }
        /*
         * Retain a reference to this Big constructor, and shadow
         * Big.prototype.constructor which points to Object.
         */
        x.constructor = Big;
    }
    Big.prototype = P;
    Big.DP = DP;
    Big.RM = RM;
    Big.E_NEG = E_NEG;
    Big.E_POS = E_POS;
    return Big;
}
// Private functions
/*
 * Return a string representing the value of Big x in normal or exponential
 * notation to dp fixed decimal places or significant digits.
 *
 * x {Big} The Big to format.
 * dp {number} Integer, 0 to MAX_DP inclusive.
 * toE {number} 1 (toExponential), 2 (toPrecision) or undefined (toFixed).
 */
function format(x, dp, toE) {
    var Big = x.constructor, 
    // The index (normal notation) of the digit that may be rounded up.
    i = dp - (x = new Big(x)).e, c = x.c;
    // Round?
    if (c.length > ++dp) {
        rnd(x, i, Big.RM);
    }
    if (!c[0]) {
        ++i;
    }
    else if (toE) {
        i = dp;
    }
    else {
        c = x.c;
        // Recalculate i as x.e may have changed if value rounded up.
        i = x.e + i + 1;
    }
    // Append zeros?
    for (; c.length < i; c.push(0)) {
    }
    i = x.e;
    /*
     * toPrecision returns exponential notation if the number of
     * significant digits specified is less than the number of digits
     * necessary to represent the integer part of the value in normal
     * notation.
     */
    return toE === 1 || toE && (dp <= i || i <= Big.E_NEG) ?
        // Exponential notation.
        (x.s < 0 && c[0] ? '-' : '') +
            (c.length > 1 ? c[0] + '.' + c.join('').slice(1) : c[0]) +
            (i < 0 ? 'e' : 'e+') + i
        : x.toString();
}
/*
 * Parse the number or string value passed to a Big constructor.
 *
 * x {Big} A Big number instance.
 * n {number|string} A numeric value.
 */
function parse(x, n) {
    var e, i, nL;
    // Minus zero?
    if (n === 0 && 1 / n < 0) {
        n = '-0';
    }
    else if (!isValid.test(n += '')) {
        throwErr(NaN);
    }
    // Determine sign.
    x.s = n.charAt(0) == '-' ? (n = n.slice(1), -1) : 1;
    // Decimal point?
    if ((e = n.indexOf('.')) > -1) {
        n = n.replace('.', '');
    }
    // Exponential form?
    if ((i = n.search(/e/i)) > 0) {
        // Determine exponent.
        if (e < 0) {
            e = i;
        }
        e += +n.slice(i + 1);
        n = n.substring(0, i);
    }
    else if (e < 0) {
        // Integer.
        e = n.length;
    }
    // Determine leading zeros.
    for (i = 0; n.charAt(i) == '0'; i++) {
    }
    if (i == (nL = n.length)) {
        // Zero.
        x.c = [x.e = 0];
    }
    else {
        // Determine trailing zeros.
        for (; n.charAt(--nL) == '0';) {
        }
        x.e = e - i - 1;
        x.c = [];
        // Convert string to array of digits without leading/trailing zeros.
        for (e = 0; i <= nL; x.c[e++] = +n.charAt(i++)) {
        }
    }
    return x;
}
/*
 * Round Big x to a maximum of dp decimal places using rounding mode rm.
 * Called by div, sqrt and round.
 *
 * x {Big} The Big to round.
 * dp {number} Integer, 0 to MAX_DP inclusive.
 * rm {number} 0, 1, 2 or 3 (DOWN, HALF_UP, HALF_EVEN, UP)
 * [more] {boolean} Whether the result of division was truncated.
 */
function rnd(x, dp, rm, more) {
    var u, xc = x.c, i = x.e + dp + 1;
    if (rm === 1) {
        // xc[i] is the digit after the digit that may be rounded up.
        more = xc[i] >= 5;
    }
    else if (rm === 2) {
        more = xc[i] > 5 || xc[i] == 5 &&
            (more || i < 0 || xc[i + 1] !== u || xc[i - 1] & 1);
    }
    else if (rm === 3) {
        more = more || xc[i] !== u || i < 0;
    }
    else {
        more = false;
        if (rm !== 0) {
            throwErr('!Big.RM!');
        }
    }
    if (i < 1 || !xc[0]) {
        if (more) {
            // 1, 0.1, 0.01, 0.001, 0.0001 etc.
            x.e = -dp;
            x.c = [1];
        }
        else {
            // Zero.
            x.c = [x.e = 0];
        }
    }
    else {
        // Remove any digits after the required decimal places.
        xc.length = i--;
        // Round up?
        if (more) {
            // Rounding up may mean the previous digit has to be rounded up.
            for (; ++xc[i] > 9;) {
                xc[i] = 0;
                if (!i--) {
                    ++x.e;
                    xc.unshift(1);
                }
            }
        }
        // Remove trailing zeros.
        for (i = xc.length; !xc[--i]; xc.pop()) {
        }
    }
    return x;
}
/*
 * Throw a BigError.
 *
 * message {string} The error message.
 */
function throwErr(message) {
    var err = new Error(message);
    err.name = 'BigError';
    throw err;
}
// Prototype/instance methods
/*
 * Return a new Big whose value is the absolute value of this Big.
 */
P.abs = function () {
    var x = new this.constructor(this);
    x.s = 1;
    return x;
};
/*
 * Return
 * 1 if the value of this Big is greater than the value of Big y,
 * -1 if the value of this Big is less than the value of Big y, or
 * 0 if they have the same value.
 */
P.cmp = function (y) {
    var xNeg, x = this, xc = x.c, yc = (y = new x.constructor(y)).c, i = x.s, j = y.s, k = x.e, l = y.e;
    // Either zero?
    if (!xc[0] || !yc[0]) {
        return !xc[0] ? !yc[0] ? 0 : -j : i;
    }
    // Signs differ?
    if (i != j) {
        return i;
    }
    xNeg = i < 0;
    // Compare exponents.
    if (k != l) {
        return k > l ^ xNeg ? 1 : -1;
    }
    i = -1;
    j = (k = xc.length) < (l = yc.length) ? k : l;
    // Compare digit by digit.
    for (; ++i < j;) {
        if (xc[i] != yc[i]) {
            return xc[i] > yc[i] ^ xNeg ? 1 : -1;
        }
    }
    // Compare lengths.
    return k == l ? 0 : k > l ^ xNeg ? 1 : -1;
};
/*
 * Return a new Big whose value is the value of this Big divided by the
 * value of Big y, rounded, if necessary, to a maximum of Big.DP decimal
 * places using rounding mode Big.RM.
 */
P.div = function (y) {
    var x = this, Big = x.constructor, 
    // dividend
    dvd = x.c, 
    //divisor
    dvs = (y = new Big(y)).c, s = x.s == y.s ? 1 : -1, dp = Big.DP;
    if (dp !== ~~dp || dp < 0 || dp > MAX_DP) {
        throwErr('!Big.DP!');
    }
    // Either 0?
    if (!dvd[0] || !dvs[0]) {
        // If both are 0, throw NaN
        if (dvd[0] == dvs[0]) {
            throwErr(NaN);
        }
        // If dvs is 0, throw +-Infinity.
        if (!dvs[0]) {
            throwErr(s / 0);
        }
        // dvd is 0, return +-0.
        return new Big(s * 0);
    }
    var dvsL, dvsT, next, cmp, remI, u, dvsZ = dvs.slice(), dvdI = dvsL = dvs.length, dvdL = dvd.length, 
    // remainder
    rem = dvd.slice(0, dvsL), remL = rem.length, 
    // quotient
    q = y, qc = q.c = [], qi = 0, digits = dp + (q.e = x.e - y.e) + 1;
    q.s = s;
    s = digits < 0 ? 0 : digits;
    // Create version of divisor with leading zero.
    dvsZ.unshift(0);
    // Add zeros to make remainder as long as divisor.
    for (; remL++ < dvsL; rem.push(0)) {
    }
    do {
        // 'next' is how many times the divisor goes into current remainder.
        for (next = 0; next < 10; next++) {
            // Compare divisor and remainder.
            if (dvsL != (remL = rem.length)) {
                cmp = dvsL > remL ? 1 : -1;
            }
            else {
                for (remI = -1, cmp = 0; ++remI < dvsL;) {
                    if (dvs[remI] != rem[remI]) {
                        cmp = dvs[remI] > rem[remI] ? 1 : -1;
                        break;
                    }
                }
            }
            // If divisor < remainder, subtract divisor from remainder.
            if (cmp < 0) {
                // Remainder can't be more than 1 digit longer than divisor.
                // Equalise lengths using divisor with extra leading zero?
                for (dvsT = remL == dvsL ? dvs : dvsZ; remL;) {
                    if (rem[--remL] < dvsT[remL]) {
                        remI = remL;
                        for (; remI && !rem[--remI]; rem[remI] = 9) {
                        }
                        --rem[remI];
                        rem[remL] += 10;
                    }
                    rem[remL] -= dvsT[remL];
                }
                for (; !rem[0]; rem.shift()) {
                }
            }
            else {
                break;
            }
        }
        // Add the 'next' digit to the result array.
        qc[qi++] = cmp ? next : ++next;
        // Update the remainder.
        if (rem[0] && cmp) {
            rem[remL] = dvd[dvdI] || 0;
        }
        else {
            rem = [dvd[dvdI]];
        }
    } while ((dvdI++ < dvdL || rem[0] !== u) && s--);
    // Leading zero? Do not remove if result is simply zero (qi == 1).
    if (!qc[0] && qi != 1) {
        // There can't be more than one zero.
        qc.shift();
        q.e--;
    }
    // Round?
    if (qi > digits) {
        rnd(q, dp, Big.RM, rem[0] !== u);
    }
    return q;
};
/*
 * Return true if the value of this Big is equal to the value of Big y,
 * otherwise returns false.
 */
P.eq = function (y) {
    return !this.cmp(y);
};
/*
 * Return true if the value of this Big is greater than the value of Big y,
 * otherwise returns false.
 */
P.gt = function (y) {
    return this.cmp(y) > 0;
};
/*
 * Return true if the value of this Big is greater than or equal to the
 * value of Big y, otherwise returns false.
 */
P.gte = function (y) {
    return this.cmp(y) > -1;
};
/*
 * Return true if the value of this Big is less than the value of Big y,
 * otherwise returns false.
 */
P.lt = function (y) {
    return this.cmp(y) < 0;
};
/*
 * Return true if the value of this Big is less than or equal to the value
 * of Big y, otherwise returns false.
 */
P.lte = function (y) {
    return this.cmp(y) < 1;
};
/*
 * Return a new Big whose value is the value of this Big minus the value
 * of Big y.
 */
P.sub = P.minus = function (y) {
    var i, j, t, xLTy, x = this, Big = x.constructor, a = x.s, b = (y = new Big(y)).s;
    // Signs differ?
    if (a != b) {
        y.s = -b;
        return x.plus(y);
    }
    var xc = x.c.slice(), xe = x.e, yc = y.c, ye = y.e;
    // Either zero?
    if (!xc[0] || !yc[0]) {
        // y is non-zero? x is non-zero? Or both are zero.
        return yc[0] ? (y.s = -b, y) : new Big(xc[0] ? x : 0);
    }
    // Determine which is the bigger number.
    // Prepend zeros to equalise exponents.
    if (a = xe - ye) {
        if (xLTy = a < 0) {
            a = -a;
            t = xc;
        }
        else {
            ye = xe;
            t = yc;
        }
        t.reverse();
        for (b = a; b--; t.push(0)) {
        }
        t.reverse();
    }
    else {
        // Exponents equal. Check digit by digit.
        j = ((xLTy = xc.length < yc.length) ? xc : yc).length;
        for (a = b = 0; b < j; b++) {
            if (xc[b] != yc[b]) {
                xLTy = xc[b] < yc[b];
                break;
            }
        }
    }
    // x < y? Point xc to the array of the bigger number.
    if (xLTy) {
        t = xc;
        xc = yc;
        yc = t;
        y.s = -y.s;
    }
    /*
     * Append zeros to xc if shorter. No need to add zeros to yc if shorter
     * as subtraction only needs to start at yc.length.
     */
    if ((b = (j = yc.length) - (i = xc.length)) > 0) {
        for (; b--; xc[i++] = 0) {
        }
    }
    // Subtract yc from xc.
    for (b = i; j > a;) {
        if (xc[--j] < yc[j]) {
            for (i = j; i && !xc[--i]; xc[i] = 9) {
            }
            --xc[i];
            xc[j] += 10;
        }
        xc[j] -= yc[j];
    }
    // Remove trailing zeros.
    for (; xc[--b] === 0; xc.pop()) {
    }
    // Remove leading zeros and adjust exponent accordingly.
    for (; xc[0] === 0;) {
        xc.shift();
        --ye;
    }
    if (!xc[0]) {
        // n - n = +0
        y.s = 1;
        // Result must be zero.
        xc = [ye = 0];
    }
    y.c = xc;
    y.e = ye;
    return y;
};
/*
 * Return a new Big whose value is the value of this Big modulo the
 * value of Big y.
 */
P.mod = function (y) {
    var yGTx, x = this, Big = x.constructor, a = x.s, b = (y = new Big(y)).s;
    if (!y.c[0]) {
        throwErr(NaN);
    }
    x.s = y.s = 1;
    yGTx = y.cmp(x) == 1;
    x.s = a;
    y.s = b;
    if (yGTx) {
        return new Big(x);
    }
    a = Big.DP;
    b = Big.RM;
    Big.DP = Big.RM = 0;
    x = x.div(y);
    Big.DP = a;
    Big.RM = b;
    return this.minus(x.times(y));
};
/*
 * Return a new Big whose value is the value of this Big plus the value
 * of Big y.
 */
P.add = P.plus = function (y) {
    var t, x = this, Big = x.constructor, a = x.s, b = (y = new Big(y)).s;
    // Signs differ?
    if (a != b) {
        y.s = -b;
        return x.minus(y);
    }
    var xe = x.e, xc = x.c, ye = y.e, yc = y.c;
    // Either zero?
    if (!xc[0] || !yc[0]) {
        // y is non-zero? x is non-zero? Or both are zero.
        return yc[0] ? y : new Big(xc[0] ? x : a * 0);
    }
    xc = xc.slice();
    // Prepend zeros to equalise exponents.
    // Note: Faster to use reverse then do unshifts.
    if (a = xe - ye) {
        if (a > 0) {
            ye = xe;
            t = yc;
        }
        else {
            a = -a;
            t = xc;
        }
        t.reverse();
        for (; a--; t.push(0)) {
        }
        t.reverse();
    }
    // Point xc to the longer array.
    if (xc.length - yc.length < 0) {
        t = yc;
        yc = xc;
        xc = t;
    }
    a = yc.length;
    /*
     * Only start adding at yc.length - 1 as the further digits of xc can be
     * left as they are.
     */
    for (b = 0; a;) {
        b = (xc[--a] = xc[a] + yc[a] + b) / 10 | 0;
        xc[a] %= 10;
    }
    // No need to check for zero, as +x + +y != 0 && -x + -y != 0
    if (b) {
        xc.unshift(b);
        ++ye;
    }
    // Remove trailing zeros.
    for (a = xc.length; xc[--a] === 0; xc.pop()) {
    }
    y.c = xc;
    y.e = ye;
    return y;
};
/*
 * Return a Big whose value is the value of this Big raised to the power n.
 * If n is negative, round, if necessary, to a maximum of Big.DP decimal
 * places using rounding mode Big.RM.
 *
 * n {number} Integer, -MAX_POWER to MAX_POWER inclusive.
 */
P.pow = function (n) {
    var x = this, one = new x.constructor(1), y = one, isNeg = n < 0;
    if (n !== ~~n || n < -MAX_POWER || n > MAX_POWER) {
        throwErr('!pow!');
    }
    n = isNeg ? -n : n;
    for (;;) {
        if (n & 1) {
            y = y.times(x);
        }
        n >>= 1;
        if (!n) {
            break;
        }
        x = x.times(x);
    }
    return isNeg ? one.div(y) : y;
};
/*
 * Return a new Big whose value is the value of this Big rounded to a
 * maximum of dp decimal places using rounding mode rm.
 * If dp is not specified, round to 0 decimal places.
 * If rm is not specified, use Big.RM.
 *
 * [dp] {number} Integer, 0 to MAX_DP inclusive.
 * [rm] 0, 1, 2 or 3 (ROUND_DOWN, ROUND_HALF_UP, ROUND_HALF_EVEN, ROUND_UP)
 */
P.round = function (dp, rm) {
    var x = this, Big = x.constructor;
    if (dp == null) {
        dp = 0;
    }
    else if (dp !== ~~dp || dp < 0 || dp > MAX_DP) {
        throwErr('!round!');
    }
    rnd(x = new Big(x), dp, rm == null ? Big.RM : rm);
    return x;
};
/*
 * Return a new Big whose value is the square root of the value of this Big,
 * rounded, if necessary, to a maximum of Big.DP decimal places using
 * rounding mode Big.RM.
 */
P.sqrt = function () {
    var estimate, r, approx, x = this, Big = x.constructor, xc = x.c, i = x.s, e = x.e, half = new Big('0.5');
    // Zero?
    if (!xc[0]) {
        return new Big(x);
    }
    // If negative, throw NaN.
    if (i < 0) {
        throwErr(NaN);
    }
    // Estimate.
    i = Math.sqrt(x.toString());
    // Math.sqrt underflow/overflow?
    // Pass x to Math.sqrt as integer, then adjust the result exponent.
    if (i === 0 || i === 1 / 0) {
        estimate = xc.join('');
        if (!(estimate.length + e & 1)) {
            estimate += '0';
        }
        r = new Big(Math.sqrt(estimate).toString());
        r.e = ((e + 1) / 2 | 0) - (e < 0 || e & 1);
    }
    else {
        r = new Big(i.toString());
    }
    i = r.e + (Big.DP += 4);
    // Newton-Raphson iteration.
    do {
        approx = r;
        r = half.times(approx.plus(x.div(approx)));
    } while (approx.c.slice(0, i).join('') !==
        r.c.slice(0, i).join(''));
    rnd(r, Big.DP -= 4, Big.RM);
    return r;
};
/*
 * Return a new Big whose value is the value of this Big times the value of
 * Big y.
 */
P.mul = P.times = function (y) {
    var c, x = this, Big = x.constructor, xc = x.c, yc = (y = new Big(y)).c, a = xc.length, b = yc.length, i = x.e, j = y.e;
    // Determine sign of result.
    y.s = x.s == y.s ? 1 : -1;
    // Return signed 0 if either 0.
    if (!xc[0] || !yc[0]) {
        return new Big(y.s * 0);
    }
    // Initialise exponent of result as x.e + y.e.
    y.e = i + j;
    // If array xc has fewer digits than yc, swap xc and yc, and lengths.
    if (a < b) {
        c = xc;
        xc = yc;
        yc = c;
        j = a;
        a = b;
        b = j;
    }
    // Initialise coefficient array of result with zeros.
    for (c = new Array(j = a + b); j--; c[j] = 0) {
    }
    // Multiply.
    // i is initially xc.length.
    for (i = b; i--;) {
        b = 0;
        // a is yc.length.
        for (j = a + i; j > i;) {
            // Current sum of products at this digit position, plus carry.
            b = c[j] + yc[i] * xc[j - i - 1] + b;
            c[j--] = b % 10;
            // carry
            b = b / 10 | 0;
        }
        c[j] = (c[j] + b) % 10;
    }
    // Increment result exponent if there is a final carry.
    if (b) {
        ++y.e;
    }
    // Remove any leading zero.
    if (!c[0]) {
        c.shift();
    }
    // Remove trailing zeros.
    for (i = c.length; !c[--i]; c.pop()) {
    }
    y.c = c;
    return y;
};
/*
 * Return a string representing the value of this Big.
 * Return exponential notation if this Big has a positive exponent equal to
 * or greater than Big.E_POS, or a negative exponent equal to or less than
 * Big.E_NEG.
 */
P.toString = P.valueOf = P.toJSON = function () {
    var x = this, Big = x.constructor, e = x.e, str = x.c.join(''), strL = str.length;
    // Exponential notation?
    if (e <= Big.E_NEG || e >= Big.E_POS) {
        str = str.charAt(0) + (strL > 1 ? '.' + str.slice(1) : '') +
            (e < 0 ? 'e' : 'e+') + e;
    }
    else if (e < 0) {
        // Prepend zeros.
        for (; ++e; str = '0' + str) {
        }
        str = '0.' + str;
    }
    else if (e > 0) {
        if (++e > strL) {
            // Append zeros.
            for (e -= strL; e--; str += '0') {
            }
        }
        else if (e < strL) {
            str = str.slice(0, e) + '.' + str.slice(e);
        }
    }
    else if (strL > 1) {
        str = str.charAt(0) + '.' + str.slice(1);
    }
    // Avoid '-0'
    return x.s < 0 && x.c[0] ? '-' + str : str;
};
/*
 ***************************************************************************
 * If toExponential, toFixed, toPrecision and format are not required they
 * can safely be commented-out or deleted. No redundant code will be left.
 * format is used only by toExponential, toFixed and toPrecision.
 ***************************************************************************
 */
/*
 * Return a string representing the value of this Big in exponential
 * notation to dp fixed decimal places and rounded, if necessary, using
 * Big.RM.
 *
 * [dp] {number} Integer, 0 to MAX_DP inclusive.
 */
P.toExponential = function (dp) {
    if (dp == null) {
        dp = this.c.length - 1;
    }
    else if (dp !== ~~dp || dp < 0 || dp > MAX_DP) {
        throwErr('!toExp!');
    }
    return format(this, dp, 1);
};
/*
 * Return a string representing the value of this Big in normal notation
 * to dp fixed decimal places and rounded, if necessary, using Big.RM.
 *
 * [dp] {number} Integer, 0 to MAX_DP inclusive.
 */
P.toFixed = function (dp) {
    var str, x = this, Big = x.constructor, neg = Big.E_NEG, pos = Big.E_POS;
    // Prevent the possibility of exponential notation.
    Big.E_NEG = -(Big.E_POS = 1 / 0);
    if (dp == null) {
        str = x.toString();
    }
    else if (dp === ~~dp && dp >= 0 && dp <= MAX_DP) {
        str = format(x, x.e + dp);
        // (-0).toFixed() is '0', but (-0.1).toFixed() is '-0'.
        // (-0).toFixed(1) is '0.0', but (-0.01).toFixed(1) is '-0.0'.
        if (x.s < 0 && x.c[0] && str.indexOf('-') < 0) {
            //E.g. -0.5 if rounded to -0 will cause toString to omit the minus sign.
            str = '-' + str;
        }
    }
    Big.E_NEG = neg;
    Big.E_POS = pos;
    if (!str) {
        throwErr('!toFix!');
    }
    return str;
};
/*
 * Return a string representing the value of this Big rounded to sd
 * significant digits using Big.RM. Use exponential notation if sd is less
 * than the number of digits necessary to represent the integer part of the
 * value in normal notation.
 *
 * sd {number} Integer, 1 to MAX_DP inclusive.
 */
P.toPrecision = function (sd) {
    if (sd == null) {
        return this.toString();
    }
    else if (sd !== ~~sd || sd < 1 || sd > MAX_DP) {
        throwErr('!toPre!');
    }
    return format(this, sd - 1, 2);
};
// Export
Big = bigFactory();
///<reference path="big.js"/>
var BigFactory = (function () {
    function BigFactory() {
    }
    BigFactory.create = function (bigDecimal) {
        if (bigDecimal === void 0) { bigDecimal = 0.0; }
        return new Big(bigDecimal);
    };
    return BigFactory;
}());
///<reference path="big.js"/>
///<reference path="big-def.ts"/>
var Note = (function () {
    function Note(pitch, start, duration, velocity, muted) {
        this.pitch = pitch;
        this.start = new Big(start);
        this.duration = new Big(duration);
        this.velocity = velocity;
        this.muted = muted;
    }
    Note.prototype.toString = function () {
        return "\n        pitch: " + this.getPitch() + "\n        start: " + this.getStartAsString() + "\n        duration: " + this.getDurationAsString() + "\n        velocity: " + this.getVelocity() + "\n        muted: " + this.getMuted();
    };
    Note.prototype.getPitch = function () {
        if (this.pitch < 0) {
            return 0;
        }
        if (this.pitch > 127) {
            return 127;
        }
        return this.pitch;
    };
    Note.prototype.getStartAsString = function () {
        // if (this.start.lt(BigFactory.create(0))) return "0.0";
        return this.start.toFixed(4);
    };
    Note.prototype.setStart = function (start) {
        this.start = start;
    };
    Note.prototype.setPitch = function (pitch) {
        if (pitch > 127) {
            pitch = 127;
        }
        if (pitch < 0) {
            pitch = 0;
        }
        this.pitch = pitch;
    };
    Note.prototype.getStart = function () {
        return this.start;
    };
    Note.prototype.getDuration = function () {
        return this.duration;
    };
    Note.prototype.getDurationAsString = function () {
        if (this.duration.lt(Note.MIN_DURATION))
            return Note.MIN_DURATION.toFixed(4);
        return this.duration.toFixed(4);
    };
    Note.prototype.getVelocity = function () {
        if (this.velocity < 0)
            return 0;
        if (this.velocity > 127)
            return 127;
        return this.velocity;
    };
    Note.prototype.getMuted = function () {
        return this.muted;
    };
    return Note;
}());
Note.NOTE_NAMES = ['C-2', 'C#-2', 'D-2', 'D#-2', 'E-2', 'F-2', 'F#-2', 'G-2', 'G#-2', 'A-2', 'A#-2', 'B-2', 'C-1', 'C#-1', 'D-1', 'D#-1', 'E-1', 'F-1', 'F#-1', 'G-1', 'G#-1', 'A-1', 'A#-1', 'B-1', 'C0', 'C#0', 'D0', 'D#0', 'E0', 'F0', 'F#0', 'G0', 'G#0', 'A0', 'A#0', 'B0', 'C1', 'C#1', 'D1', 'D#1', 'E1', 'F1', 'F#1', 'G1', 'G#1', 'A1', 'A#1', 'B1', 'C2', 'C#2', 'D2', 'D#2', 'E2', 'F2', 'F#2', 'G2', 'G#2', 'A2', 'A#2', 'B2', 'C3', 'C#3', 'D3', 'D#3', 'E3', 'F3', 'F#3', 'G3', 'G#3', 'A3', 'A#3', 'B3', 'C4', 'C#4', 'D4', 'D#4', 'E4', 'F4', 'F#4', 'G4', 'G#4', 'A4', 'A#4', 'B4', 'C5', 'C#5', 'D5', 'D#5', 'E5', 'F5', 'F#5', 'G5', 'G#5', 'A5', 'A#5', 'B5', 'C6', 'C#6', 'D6', 'D#6', 'E6', 'F6', 'F#6', 'G6', 'G#6', 'A6', 'A#6', 'B6', 'C7', 'C#7', 'D7', 'D#7', 'E7', 'F7', 'F#7', 'G7', 'G#7', 'A7', 'A#7', 'B7', 'C8', 'C#8', 'D8', 'D#8', 'E8', 'F8', 'F#8', 'G8'];
Note.MIN_DURATION = new Big(1 / 128);
///<reference path="big.js"/>
///<reference path="big-def.ts"/>
///<reference path="Note.ts"/>
var Clip = (function () {
    function Clip(path) {
        if (path === void 0) { path = "live_set view highlighted_clip_slot clip"; }
        this.liveObject = new LiveAPI(path);
    }
    Clip.prototype.getLength = function () {
        return BigFactory.create(this.liveObject.get('length'));
    };
    Clip.parseNoteData = function (data) {
        var notes = [];
        // data starts with "notes"/count and ends with "done" (which we ignore)
        for (var i = 2, len = data.length - 1; i < len; i += 6) {
            // and each note starts with "note" (which we ignore) and is 6 items in the list
            var note = new Note(data[i + 1 /* pitch */], data[i + 2 /* start */], data[i + 3 /* duration */], data[i + 4 /* velocity */], data[i + 5 /* muted */]);
            notes.push(note);
        }
        return notes;
    };
    Clip.prototype.getSelectedNotes = function () {
        var data = this.liveObject.call('get_selected_notes');
        return Clip.parseNoteData(data);
    };
    Clip.prototype.getNotes = function (startTime, timeRange, startPitch, pitchRange) {
        if (startTime === void 0) { startTime = 0; }
        if (timeRange === void 0) { timeRange = this.getLength(); }
        if (startPitch === void 0) { startPitch = 0; }
        if (pitchRange === void 0) { pitchRange = 128; }
        var data = this.liveObject.call("get_notes", startTime, startPitch, timeRange.toFixed(4), pitchRange);
        return Clip.parseNoteData(data);
    };
    Clip.prototype.sendNotes = function (notes) {
        var liveObject = this.liveObject;
        liveObject.call("notes", notes.length);
        notes.forEach(function (note) {
            liveObject.call("note", note.getPitch(), note.getStartAsString(), note.getDurationAsString(), note.getVelocity(), note.getMuted());
        });
        liveObject.call('done');
    };
    Clip.prototype.replaceSelectedNotes = function (notes) {
        this.liveObject.call("replace_selected_notes");
        this.sendNotes(notes);
    };
    Clip.prototype.setNotes = function (notes) {
        this.liveObject.call("set_notes");
        this.sendNotes(notes);
    };
    Clip.prototype.selectAllNotes = function () {
        this.liveObject.call("select_all_notes");
    };
    Clip.prototype.replaceAllNotes = function (notes) {
        this.selectAllNotes();
        this.replaceSelectedNotes(notes);
    };
    return Clip;
}());
///<reference path="Note.ts"/>
var Action;
(function (Action) {
    Action[Action["Constrain"] = 0] = "Constrain";
    Action[Action["Transpose"] = 1] = "Transpose";
    Action[Action["Monophonize"] = 2] = "Monophonize";
    Action[Action["Fractalize"] = 3] = "Fractalize";
    Action[Action["Mix"] = 4] = "Mix";
    Action[Action["Interleave"] = 5] = "Interleave";
})(Action || (Action = {}));
var InterleaveMode;
(function (InterleaveMode) {
    InterleaveMode[InterleaveMode["EventCount"] = 0] = "EventCount";
    InterleaveMode[InterleaveMode["TimeRange"] = 1] = "TimeRange"; // e.g. interleave 1/4 from A for every 1/8 from B
})(InterleaveMode || (InterleaveMode = {}));
var ClipActions = (function () {
    function ClipActions() {
        this.noteDurations = {};
        var barLength = new Big(4);
        this.noteDurations["1"] = barLength;
        this.noteDurations["1/2"] = barLength.div(new Big(2));
        this.noteDurations["1/4"] = barLength.div(new Big(4));
        this.noteDurations["1/8"] = barLength.div(new Big(8));
        this.noteDurations["1/16"] = barLength.div(new Big(16));
        this.noteDurations["1/32"] = barLength.div(new Big(32));
        this.actions = [];
        this.actions[Action.Constrain] = function (notesToMutate, notesToSourceFrom, options) {
            var results = [];
            for (var _i = 0, notesToMutate_1 = notesToMutate; _i < notesToMutate_1.length; _i++) {
                var note = notesToMutate_1[_i];
                var result = note;
                if (options.constrainNotePitch) {
                    result.setPitch(ClipActions.findNearestNotePitchInSet(note, notesToSourceFrom));
                }
                if (options.constrainNoteStart) {
                    result.setStart(ClipActions.findNearestNoteStartInSet(note, notesToSourceFrom));
                }
                results.push(result);
            }
            return results;
        };
        this.actions[Action.Interleave] = function (a, b, options) {
            var results = [];
            ClipActions.sortNotes(a);
            ClipActions.sortNotes(b);
            var position = new Big(0);
            if (options.interleaveMode === InterleaveMode.EventCount) {
                var i = 0;
                while (i < b.length || i < a.length) {
                    var ca = a[i % a.length], cb = b[i % b.length];
                    // if i = 0 for a, update pos
                    // add a at pos
                    // update pos with next a
                    // if i = 0 for next a, calculate distance from start of event to end of clip and add to pos
                    // add b at pos
                    // update pos with next b
                    // if i = 0 for next b, calculate distance from start of event to end of clip and add to pos
                    if (i % a.length === 0) {
                        position = position.plus(ca.getStart());
                    }
                    //console.log(i % a.length, a[(i + 1) % a.length].getStartAsString(), position.toFixed(4), ca.getStartAsString());
                    ca.setStart(position);
                    results.push(ca);
                    position = position.plus(a[(i + 1) % a.length].getStart()).minus(ca.getStart());
                    // todo: if ((i + 1) % a.length === 0) {
                    cb.setStart(position);
                    results.push(cb);
                    position = position.plus(b[(i + 1) % b.length].getStart()).minus(cb.getStart());
                    i++;
                }
            }
            return results;
        };
    }
    ClipActions.prototype.process = function (action, notesToMutate, notesToSourceFrom, options) {
        return this.actions[action](notesToMutate, notesToSourceFrom, options);
    };
    ClipActions.findNearestNoteStartInSet = function (needle, haystack) {
        var nearestIndex = 0, nearestDelta;
        for (var i = 0; i < haystack.length; i++) {
            if (nearestDelta === undefined) {
                nearestDelta = needle.getStart().minus(haystack[i].getStart()).abs();
            }
            var currentDelta = needle.getStart().minus(haystack[i].getStart()).abs();
            if (currentDelta.lt(nearestDelta)) {
                nearestDelta = currentDelta;
                nearestIndex = i;
            }
        }
        return haystack[nearestIndex].getStart();
    };
    ClipActions.findNearestNotePitchInSet = function (needle, haystack) {
        var nearestIndex = 0, nearestDelta;
        for (var i = 0; i < haystack.length; i++) {
            if (nearestDelta === undefined) {
                nearestDelta = Math.abs(needle.getPitch() - haystack[i].getPitch());
            }
            var currentDelta = Math.abs(needle.getPitch() - haystack[i].getPitch());
            if (currentDelta < nearestDelta) {
                nearestDelta = currentDelta;
                nearestIndex = i;
            }
        }
        return haystack[nearestIndex].getPitch();
    };
    // sorts notes according to position
    ClipActions.sortNotes = function (notes) {
        notes = notes.sort(function (a, b) {
            if (a.getStart().lt(b.getStart())) {
                return -1;
            }
            if (a.getStart().gt(b.getStart())) {
                return 1;
            }
            return 0;
        });
        /*
                notes = notes.sort((a: Note, b: Note) => {
                    if (a.getPitch() < b.getPitch()) {
                        return -1;
                    }
                    if (a.getPitch() > b.getPitch()) {
                        return 1;
                    }
                    return 0;
                });
        */
    };
    return ClipActions;
}());
///<reference path="ClipActions.ts"/>
///<reference path="Clip.ts"/>
var ClipProcessor = (function () {
    function ClipProcessor() {
        this.defaultOptions = {
            constrainNotePitch: true,
            constrainNoteStart: false,
            interleaveMode: InterleaveMode.EventCount,
            interleaveEventCountA: 1,
            interleaveEventCountB: 1,
            interleaveEventRangeA: new Big(1),
            interleaveEventRangeB: new Big(1)
        };
        this.clipActions = new ClipActions();
        this.options = this.getDefaultOptions();
    }
    ClipProcessor.prototype.getDefaultOptions = function () {
        var options = {};
        for (var _i = 0, _a = Object.keys(this.defaultOptions); _i < _a.length; _i++) {
            var option = _a[_i];
            options[option] = this.defaultOptions[option];
        }
        return options;
    };
    ClipProcessor.prototype.setClipToMutate = function (clip) {
        if (clip === void 0) { clip = new Clip(); }
        this.clipToMutate = clip;
    };
    ClipProcessor.prototype.setClipToSourceFrom = function (clip) {
        if (clip === void 0) { clip = new Clip(); }
        this.clipToSourceFrom = clip;
    };
    ClipProcessor.prototype.setAction = function (action) {
        this.action = action;
    };
    // Sets option. 1 = true, 0 = false
    ClipProcessor.prototype.setOption = function (optionName, value) {
        if (this.options[optionName] !== undefined) {
            this.options[optionName] = (value === 1);
        }
    };
    ClipProcessor.prototype.processClip = function () {
        if (!this.clipToMutate || !this.clipToSourceFrom)
            return;
        var notesToMutate = this.clipToMutate.getNotes();
        var notesToSourceFrom = this.clipToSourceFrom.getNotes();
        if (notesToMutate.length === 0 || notesToSourceFrom.length === 0)
            return;
        // todo: selection logic goes here...
        // console.log("processClip");
        var mutatedNotes = this.clipActions.process(this.action, notesToMutate, notesToSourceFrom, this.options);
        this.clipToMutate.setNotes(mutatedNotes);
    };
    return ClipProcessor;
}());
///<reference path="big-def.ts"/>
///<reference path="Clip.ts"/>
///<reference path="ClipProcessor.ts"/>
outlets = 1;
inlets = 1;
var clipProcessor = new ClipProcessor();
function bang() {
    var clp = new Clip();
    // clp.selectAllNotes();
    // var notes: Note[] = clp.getSelectedNotes();
    var notes = clp.getNotes();
    for (var _i = 0, notes_1 = notes; _i < notes_1.length; _i++) {
        var note = notes_1[_i];
        post(note.toString());
    }
}
function getvalueof() {
    return JSON.stringify(clipProcessor.options);
}
function setvalueof(data) {
    if (data === 0) {
        clipProcessor.options = clipProcessor.getDefaultOptions();
    }
    else {
        clipProcessor.options = JSON.parse(data);
    }
    for (var _i = 0, _a = Object.keys(clipProcessor.options); _i < _a.length; _i++) {
        var option = _a[_i];
        outlet(0, [option, clipProcessor.options[option]]);
    }
}
function setClipToMutate() {
    clipProcessor.setClipToMutate();
}
function setClipToSourceFrom() {
    clipProcessor.setClipToSourceFrom();
}
function setAction(action) {
    var ix = Action[action];
    if (ix !== void 0) {
        clipProcessor.setAction(ix);
    }
}
function setOption(key, value) {
    clipProcessor.setOption(key, value);
    notifyclients();
}
function process() {
    clipProcessor.processClip();
}
/*
function applyConstrainNoteStart(source: Clip, dest: Clip) {
    clipProcessor.processClip(Action.Constrain, {constrainNoteStart: true, constrainNotePitch: false});
}

function applyConstrainNotePitch(source: Clip, dest: Clip) {
    clipProcessor.processClip(Action.Constrain, {constrainNoteStart: true, constrainNotePitch: false});
}
*/
