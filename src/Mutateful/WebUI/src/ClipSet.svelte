<script lang="ts">
    import ClipSlot from "./ClipSlot.svelte";
</script>

<div class="cells">
    <ClipSlot clipRef="A1"/>
    <ClipSlot clipRef="A2"/>
    <ClipSlot clipRef="A3"/>
    <ClipSlot clipRef="A4"/>
    <ClipSlot clipRef="A5"/>
    <ClipSlot clipRef="A6"/>
    <ClipSlot clipRef="A7"/>
    <ClipSlot clipRef="A8"/>
    <ClipSlot clipRef="A9"/>
    <ClipSlot clipRef="A10"/>
    <ClipSlot clipRef="A11"/>
    <ClipSlot clipRef="A12"/>
    <ClipSlot clipRef="A13"/>
    <ClipSlot clipRef="A14"/>
</div>

<style>
    .cells {
        position: relative;
        display: grid;
        grid-template-columns: repeat(11, calc(100% / 11));
        grid-template-rows: repeat(21, 25px);
        grid-gap: 2px;
        background-color: #6c3f3f;
        grid-auto-flow: dense;
        max-width: 100%;
        overflow: hidden;
        padding: 2px;
    }

/*    &__input {
         border: none;
         padding: 0 6px;
     }
    input, button {
        border: none;
        background: #fff;
        padding: 0 6px;
        font-family: 'Noto Sans', sans-serif;
    }*/
</style>