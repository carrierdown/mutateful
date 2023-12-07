<script lang="ts">
    import {clipDataStore} from "./stores";
    import {onMount} from "svelte";
    import {decodeClip} from "./dataHelpers";
    import {Clip} from "./clip";
    export let clipRef;
    export let formula = "";

    let canvas;
    let canvasWidth = 300;
    let canvasHeight = 150;
    let empty = true;

    onMount(() => {
        let cs = getComputedStyle(canvas);
        canvasWidth = parseInt(cs.getPropertyValue('width'), 10);
        canvasHeight = parseInt(cs.getPropertyValue('height'), 10);
    });

    const updateClip = (clip: Clip) => {
        empty = false;
        const ctx = canvas.getContext('2d');
        // console.log(canvasWidth, canvasHeight);
        ctx.clearRect(0, 0, canvasWidth, canvasHeight);

        // ctx.fillStyle = "white";
        // ctx.fillRect(0, 0, canvasWidth, canvasHeight);
        ctx.fillStyle = "#B4B1B0";
        // ctx.fillStyle = "red";
        let length = Math.min(clip.length, 16);
        let pitches = clip.notes.map(x => x.pitch);
        let highestNote = Math.max(...pitches);
        let lowestNote = Math.min(...pitches);
        let noteRange = Math.max((highestNote) - lowestNote, 5);
        let xDelta = canvasWidth / length;
        let yDelta = canvasHeight / (noteRange + 1);

        for (let note of clip.notes)
        {
            if (note.start >= length) {
                console.log("skipping")
                return;
            }
            ctx.fillRect(Math.floor(xDelta * note.start), Math.floor(yDelta * (noteRange - (note.pitch - lowestNote))), Math.floor(xDelta * note.duration), Math.floor(yDelta));
        }
    };

    const getClip = (data: Uint8Array) => {
        updateClip(decodeClip(data));
        return "";
    }
</script>

<div class="clip-slot">
    <div class="clip-slot--header">
        <span class="clip-slot--ref">{clipRef.toUpperCase()}</span><span class="clip-slot--title">{formula}</span>
    </div>
    <canvas class="clip-slot--preview" class:empty width="{canvasWidth}" height="{canvasHeight}" bind:this={canvas}>{#if $clipDataStore.clipRef === clipRef}{getClip($clipDataStore.data)}{/if}</canvas>
</div>

<style lang="less">
    @clipPreviewBackground: #353535;
    @clipPreviewForeground: #B4B1B0;
    @formulaBackground: #5A5A5A;
    @formulaForeground: #DFDFDF;

    .clip-slot {
        border: none;
        background-color: @clipPreviewBackground;
    }

    .empty {
        transform: scale(0);
    }

    .clip-slot--header {
        display: flex;
        line-height: 2rem;
        white-space: nowrap;
        overflow: hidden;
        span {
            padding: 0 .4rem;
        }
    }

    .clip-slot--ref {
        background-color: #7562C9;
        color: black;
    }
    .clip-slot--title {
        flex-grow: 1;
        background-color: @formulaBackground;
        color: @formulaForeground;
    }

    .clip-slot--ref, .clip-slot--title {
        padding: .1rem .2rem;
    }

    canvas {
        background-color: @clipPreviewBackground;
        width: 100%;
        height: 50px;
        padding: 1px;
    }
</style>
