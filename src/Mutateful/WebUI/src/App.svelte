<script lang="ts">
    import ClipSet from "./ClipSet.svelte";
    import FormulaInput from "./FormulaInput.svelte";
</script>

<div class="container">
    <FormulaInput/>
    <ClipSet/>
</div>

<style>
    :global(body) {
        font-family: sans-serif;
        margin: 0;
        background-color: #202020;
        --scrollbar-width: calc(100vw - 100%);
    }
    .container {
        overflow-x: auto;
        width: calc(100vw - var(--scrollbar-width));
    }
</style>
