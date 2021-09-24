var app=function(){"use strict";function t(){}function e(t){return t()}function n(){return Object.create(null)}function o(t){t.forEach(e)}function r(t){return"function"==typeof t}function l(t,e){return t!=t?e==e:t!==e||t&&"object"==typeof t||"function"==typeof t}function c(e,n,o){e.$$.on_destroy.push(function(e,...n){if(null==e)return t;const o=e.subscribe(...n);return o.unsubscribe?()=>o.unsubscribe():o}(n,o))}function s(t,e){t.appendChild(e)}function u(t,e,n){t.insertBefore(e,n||null)}function i(t){t.parentNode.removeChild(t)}function f(t){return document.createElement(t)}function a(t){return document.createTextNode(t)}function p(){return a(" ")}function $(t,e,n){null==n?t.removeAttribute(e):t.getAttribute(e)!==n&&t.setAttribute(e,n)}function d(t,e){e=""+e,t.wholeText!==e&&(t.data=e)}let g;function m(t){g=t}function h(t){(function(){if(!g)throw new Error("Function called outside component initialization");return g})().$$.on_mount.push(t)}const b=[],w=[],y=[],_=[],x=Promise.resolve();let A=!1;function R(t){y.push(t)}let v=!1;const C=new Set;function k(){if(!v){v=!0;do{for(let t=0;t<b.length;t+=1){const e=b[t];m(e),E(e.$$)}for(m(null),b.length=0;w.length;)w.pop()();for(let t=0;t<y.length;t+=1){const e=y[t];C.has(e)||(C.add(e),e())}y.length=0}while(b.length);for(;_.length;)_.pop()();A=!1,v=!1,C.clear()}}function E(t){if(null!==t.fragment){t.update(),o(t.before_update);const e=t.dirty;t.dirty=[-1],t.fragment&&t.fragment.p(t.ctx,e),t.after_update.forEach(R)}}const U=new Set;function P(t,e){t&&t.i&&(U.delete(t),t.i(e))}function S(t,e,n,o){if(t&&t.o){if(U.has(t))return;U.add(t),undefined.c.push((()=>{U.delete(t),o&&(n&&t.d(1),o())})),t.o(e)}}function M(t){t&&t.c()}function H(t,n,l,c){const{fragment:s,on_mount:u,on_destroy:i,after_update:f}=t.$$;s&&s.m(n,l),c||R((()=>{const n=u.map(e).filter(r);i?i.push(...n):o(n),t.$$.on_mount=[]})),f.forEach(R)}function O(t,e){const n=t.$$;null!==n.fragment&&(o(n.on_destroy),n.fragment&&n.fragment.d(e),n.on_destroy=n.fragment=null,n.ctx=[])}function j(t,e){-1===t.$$.dirty[0]&&(b.push(t),A||(A=!0,x.then(k)),t.$$.dirty.fill(0)),t.$$.dirty[e/31|0]|=1<<e%31}function z(e,r,l,c,s,u,f,a=[-1]){const p=g;m(e);const $=e.$$={fragment:null,ctx:null,props:u,update:t,not_equal:s,bound:n(),on_mount:[],on_destroy:[],on_disconnect:[],before_update:[],after_update:[],context:new Map(p?p.$$.context:r.context||[]),callbacks:n(),dirty:a,skip_bound:!1,root:r.target||p.$$.root};f&&f($.root);let d=!1;if($.ctx=l?l(e,r.props||{},((t,n,...o)=>{const r=o.length?o[0]:n;return $.ctx&&s($.ctx[t],$.ctx[t]=r)&&(!$.skip_bound&&$.bound[t]&&$.bound[t](r),d&&j(e,t)),n})):[],$.update(),d=!0,o($.before_update),$.fragment=!!c&&c($.ctx),r.target){if(r.hydrate){const t=function(t){return Array.from(t.childNodes)}(r.target);$.fragment&&$.fragment.l(t),t.forEach(i)}else $.fragment&&$.fragment.c();r.intro&&P(e.$$.fragment),H(e,r.target,r.anchor,r.customElement),k()}m(p)}class F{$destroy(){O(this,1),this.$destroy=t}$on(t,e){const n=this.$$.callbacks[t]||(this.$$.callbacks[t]=[]);return n.push(e),()=>{const t=n.indexOf(e);-1!==t&&n.splice(t,1)}}$set(t){var e;this.$$set&&(e=t,0!==Object.keys(e).length)&&(this.$$.skip_bound=!0,this.$$set(t),this.$$.skip_bound=!1)}}const N=[];function B(e,n=t){let o;const r=new Set;function c(t){if(l(e,t)&&(e=t,o)){const t=!N.length;for(const t of r)t[1](),N.push(t,e);if(t){for(let t=0;t<N.length;t+=2)N[t][0](N[t+1]);N.length=0}}}return{set:c,update:function(t){c(t(e))},subscribe:function(l,s=t){const u=[l,s];return r.add(u),1===r.size&&(o=n(c)||t),l(e),()=>{r.delete(u),0===r.size&&(o(),o=null)}}}}const D=(new signalR.HubConnectionBuilder).withUrl("http://localhost:5000/mutatefulHub").withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol).build();D.start().then((()=>{console.log("Connection established")}),(()=>{console.log("Failed to connect :(")}));const I=t=>({subscribe:B(new Uint8Array,(e=>(D.on("SetClipDataOnClient",((n,o)=>{n===t&&(console.log("DebugMessage - received data: ",o),e(o))})),()=>{console.log("readableClip end called")}))).subscribe});class T{constructor(t){this.notes=[],this.length=t}}class V{constructor(t,e,n,o){}}function q(t){let e=G(t,2),n=new T(e),o=function(t,e=0){let n=new Uint8Array(2);return n[0]=t[e],n[1]=t[e+1],new Uint16Array(n.buffer)[0]}(t,7),r=9;for(let e=0;e<o;e++)n.notes.push(new V(t[r],G(t,r+1),G(t,r+5),G(t,r+9))),r+=25;return n}function G(t,e=0){let n=new Uint8Array(4);for(let o=0;o<4;o++)n[o]=t[o+e];return new Float32Array(n.buffer)[0]}function J(e){let n,o,r,l,c,g,m,h,b,w=e[0].toUpperCase()+"",y=e[4](e[2])+"";return{c(){n=p(),o=f("div"),r=f("div"),l=f("span"),c=a(w),g=a("Clip title goes here"),m=p(),h=f("canvas"),b=a(y),$(l,"class","clip-slot--ref"),$(r,"class","clip-slot--title"),$(h,"class","clip-slot--preview svelte-106dxyc"),$(o,"class","clip-slot svelte-106dxyc")},m(t,i){u(t,n,i),u(t,o,i),s(o,r),s(r,l),s(l,c),s(r,g),s(o,m),s(o,h),s(h,b),e[5](h)},p(t,[e]){1&e&&w!==(w=t[0].toUpperCase()+"")&&d(c,w),4&e&&y!==(y=t[4](t[2])+"")&&d(b,y)},i:t,o:t,d(t){t&&i(n),t&&i(o),e[5](null)}}}function K(t,e,n){let o,{clipRef:r}=e;const l=I(r||"A1");let s;c(t,l,(t=>n(2,o=t)));let u=!1,i=300,f=150;h((()=>{u=!0;let t=getComputedStyle(s);i=parseInt(t.getPropertyValue("width"),10),f=parseInt(t.getPropertyValue("height"),10)}));return t.$$set=t=>{"clipRef"in t&&n(0,r=t.clipRef)},[r,s,o,l,t=>(u&&(t=>{const e=s.getContext("2d");let n=q(t);console.log("Got clip!",n.length,n.notes.length),e.clearRect(0,0,i,f),e.fillStyle="red",e.fillRect(Math.floor(30*Math.random()),10,50,50)})(t),""),function(t){w[t?"unshift":"push"]((()=>{s=t,n(1,s)}))}]}class L extends F{constructor(t){super(),z(this,t,K,J,l,{clipRef:0})}}function Q(e){let n,o,r,l,c,a,d,g,m,h,b,w,y,_,x,A,R,v,C,k;return r=new L({props:{clipRef:"A1"}}),c=new L({props:{clipRef:"A2"}}),d=new L({props:{clipRef:"A3"}}),m=new L({props:{clipRef:"A4"}}),b=new L({props:{clipRef:"A5"}}),y=new L({props:{clipRef:"A6"}}),x=new L({props:{clipRef:"A7"}}),R=new L({props:{clipRef:"A8"}}),C=new L({props:{clipRef:"A9"}}),{c(){n=p(),o=f("div"),M(r.$$.fragment),l=p(),M(c.$$.fragment),a=p(),M(d.$$.fragment),g=p(),M(m.$$.fragment),h=p(),M(b.$$.fragment),w=p(),M(y.$$.fragment),_=p(),M(x.$$.fragment),A=p(),M(R.$$.fragment),v=p(),M(C.$$.fragment),$(o,"class","cells svelte-t4533h")},m(t,e){u(t,n,e),u(t,o,e),H(r,o,null),s(o,l),H(c,o,null),s(o,a),H(d,o,null),s(o,g),H(m,o,null),s(o,h),H(b,o,null),s(o,w),H(y,o,null),s(o,_),H(x,o,null),s(o,A),H(R,o,null),s(o,v),H(C,o,null),k=!0},p:t,i(t){k||(P(r.$$.fragment,t),P(c.$$.fragment,t),P(d.$$.fragment,t),P(m.$$.fragment,t),P(b.$$.fragment,t),P(y.$$.fragment,t),P(x.$$.fragment,t),P(R.$$.fragment,t),P(C.$$.fragment,t),k=!0)},o(t){S(r.$$.fragment,t),S(c.$$.fragment,t),S(d.$$.fragment,t),S(m.$$.fragment,t),S(b.$$.fragment,t),S(y.$$.fragment,t),S(x.$$.fragment,t),S(R.$$.fragment,t),S(C.$$.fragment,t),k=!1},d(t){t&&i(n),t&&i(o),O(r),O(c),O(d),O(m),O(b),O(y),O(x),O(R),O(C)}}}class W extends F{constructor(t){super(),z(this,t,null,Q,l,{})}}function X(e){let n,o;return n=new W({}),{c(){M(n.$$.fragment)},m(t,e){H(n,t,e),o=!0},p:t,i(t){o||(P(n.$$.fragment,t),o=!0)},o(t){S(n.$$.fragment,t),o=!1},d(t){O(n,t)}}}return new class extends F{constructor(t){super(),z(this,t,null,X,l,{})}}({target:document.body,props:{name:"world"}})}();
//# sourceMappingURL=index.js.map
