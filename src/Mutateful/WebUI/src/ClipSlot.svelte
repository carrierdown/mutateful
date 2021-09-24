<script lang="ts">
    import {clipDataStore} from "./stores";
    import {onMount} from "svelte";
    import {decodeClip} from "./dataHelpers";
    import {Clip} from "./clip";
    export let clipRef;
    const readableClip = clipDataStore(clipRef || "A1");
    
    let canvas;
    let mounted: boolean = false;
    let canvasWidth = 300;
    let canvasHeight = 150;
    
    onMount(() => {
        mounted = true;
        let cs = getComputedStyle(canvas);
        canvasWidth = parseInt(cs.getPropertyValue('width'), 10);
        canvasHeight = parseInt(cs.getPropertyValue('height'), 10);
    });
    
    const updateClip = (clip: Clip) => {
        const ctx = canvas.getContext('2d');
        console.log(canvasWidth, canvasHeight);
        ctx.clearRect(0, 0, canvasWidth, canvasHeight);
        
        ctx.fillStyle = "white";
        ctx.fillRect(0, 0, canvasWidth, canvasHeight);
        ctx.fillStyle = "red";
        let length = Math.min(clip.length, 16);
        let pitches = clip.notes.map(x => x.pitch);
        let highestNote = Math.max(...pitches);
        let lowestNote = Math.min(...pitches);
        let noteRange = (highestNote + 1) - lowestNote;
        let xDelta = canvasWidth / length;
        let yDelta = canvasHeight / noteRange;
        
        for (let note of clip.notes)
        {
            if (note.start >= length) return;
            // console.log(Math.floor(xDelta * note.start), Math.floor(yDelta * (note.pitch - lowestNote)), Math.floor(xDelta), Math.floor(yDelta));
            ctx.fillRect(Math.floor(xDelta * note.start), Math.floor(yDelta * (note.pitch - lowestNote)), Math.floor(xDelta), Math.floor(yDelta));
        }
    };

    const getClip = (data: Uint8Array) => {
        if (mounted) {
            updateClip(decodeClip(data));
        }
        return "";
    }
</script>

<div class="clip-slot">
    <div class="clip-slot--title"><span class="clip-slot--ref">{clipRef.toUpperCase()}</span>Clip title goes here</div>
    <canvas class="clip-slot--preview" width="{canvasWidth}" height="{canvasHeight}" bind:this={canvas}>{getClip($readableClip)}</canvas>
</div>

<style>
    .clip-slot {
        border: none;
        background: #fff;
        padding: 0 6px;
    }
    
    canvas {
        width: 100%;
        height: 50px;
    }
</style>