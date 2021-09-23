<script lang="ts">
    import {readableClip} from "./stores";
    export let clipRef;
    const readableSpecificClip = readableClip(clipRef);
    console.log("Clipref: ", clipRef);
</script>

<div class="clip-slot">
    <div class="clip-slot--title"><span class="clip-slot--ref">{clipRef.toUpperCase()}</span>Clip title goes here {$readableSpecificClip}</div>
    <canvas class="clip-slot--preview"></canvas>
</div>

<style>
    .clip-slot {
        border: none;
        background: #fff;
        padding: 0 6px;
    }
</style>