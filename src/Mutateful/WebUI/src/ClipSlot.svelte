<script lang="ts">
    import {clipDataStore} from "./stores";
    import {onMount} from "svelte";
    import {decodeClip} from "./dataHelpers";
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
    
    const updateClip = (data: Uint8Array) => {
        const ctx = canvas.getContext('2d');
        let clip = decodeClip(data);
        console.log("Got clip!", clip.length, clip.notes.length);
        ctx.clearRect(0, 0, canvasWidth, canvasHeight);
        ctx.fillStyle = "red";
        ctx.fillRect(Math.floor(Math.random() * 30),10,50,50);
    };
    
    const formatter = (data: Uint8Array) => {
        if (mounted) {
            updateClip(data);
        }
        return "";
    }
</script>

<div class="clip-slot">
    <div class="clip-slot--title"><span class="clip-slot--ref">{clipRef.toUpperCase()}</span>Clip title goes here</div>
    <canvas class="clip-slot--preview" bind:this={canvas}>{formatter($readableClip)}</canvas>
</div>

<style>
    .clip-slot {
        border: none;
        background: #fff;
        padding: 0 6px;
    }
    
    canvas {
        width: 100%;
    }
</style>