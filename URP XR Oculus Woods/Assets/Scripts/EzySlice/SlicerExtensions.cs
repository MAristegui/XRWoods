using System.Collections;
using UnityEngine;


public struct UVOffset
{
    public Vector2 offset;
    public Vector2 scale;
    public float value;
}

namespace EzySlice {
    /**
     * Define Extension methods for easy access to slicer functionality
     */
    public static class SlicerExtensions {

        /**
         * SlicedHull Return functions and appropriate overrides!
         */
        public static SlicedHull Slice(this GameObject obj, Plane pl, ref UVOffset uvoffset, Material crossSectionMaterial = null) {
            return Slice(obj, pl, new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f), ref uvoffset, crossSectionMaterial);
        }

        public static SlicedHull Slice(this GameObject obj, Vector3 position, Vector3 direction, ref UVOffset uvoffset, Material crossSectionMaterial = null) {
            return Slice(obj, position, direction, new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f), ref uvoffset, crossSectionMaterial);
        }

        public static SlicedHull Slice(this GameObject obj, Vector3 position, Vector3 direction, TextureRegion textureRegion, ref UVOffset uvoffset, Material crossSectionMaterial = null) {
            Plane cuttingPlane = new Plane();

            Vector3 refUp = obj.transform.InverseTransformDirection(direction);
            Vector3 refPt = obj.transform.InverseTransformPoint(position);

            cuttingPlane.Compute(refPt, refUp);

            return Slice(obj, cuttingPlane, textureRegion, ref uvoffset, crossSectionMaterial);
        }

        public static SlicedHull Slice(this GameObject obj, Plane pl, TextureRegion textureRegion, ref UVOffset uvoffset, Material crossSectionMaterial = null) {
            return Slicer.Slice(obj, pl, textureRegion, crossSectionMaterial, ref uvoffset);
        }

        /**
         * These functions (and overrides) will return the final indtaniated GameObjects types
         */
        public static GameObject[] SliceInstantiate(this GameObject obj, Plane pl, ref UVOffset uvoffset) {
            return SliceInstantiate(obj, pl, new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f), ref uvoffset);
        }

        public static GameObject[] SliceInstantiate(this GameObject obj, Vector3 position, Vector3 direction,ref UVOffset uvoffset) {
            return SliceInstantiate(obj, position, direction, ref uvoffset, null);
        }

        public static GameObject[] SliceInstantiate(this GameObject obj, Vector3 position, Vector3 direction, ref UVOffset uvoffset, Material crossSectionMat) {
            return SliceInstantiate(obj, position, direction, new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f), ref uvoffset, crossSectionMat);
        }

        public static GameObject[] SliceInstantiate(this GameObject obj, Vector3 position, Vector3 direction, TextureRegion cuttingRegion, ref UVOffset uvoffset, Material crossSectionMaterial = null) {
            EzySlice.Plane cuttingPlane = new EzySlice.Plane();

            Vector3 refUp = obj.transform.InverseTransformDirection(direction);
            Vector3 refPt = obj.transform.InverseTransformPoint(position);

            cuttingPlane.Compute(refPt, refUp);

            return SliceInstantiate(obj, cuttingPlane, cuttingRegion, ref uvoffset, crossSectionMaterial);
        }

        public static GameObject[] SliceInstantiate(this GameObject obj, Plane pl, TextureRegion cuttingRegion, ref UVOffset uvoffset, Material crossSectionMaterial = null) {
            SlicedHull slice = Slicer.Slice(obj, pl, cuttingRegion, crossSectionMaterial, ref uvoffset);

            if (slice == null) {
                return null;
            }

            GameObject upperHull = slice.CreateUpperHull(obj, crossSectionMaterial);
            GameObject lowerHull = slice.CreateLowerHull(obj, crossSectionMaterial);

            if (upperHull != null && lowerHull != null) {
                return new GameObject[] { upperHull, lowerHull };
            }

            // otherwise return only the upper hull
            if (upperHull != null) {
                return new GameObject[] { upperHull };
            }

            // otherwise return only the lower hull
            if (lowerHull != null) {
                return new GameObject[] { lowerHull };
            }

            // nothing to return, so return nothing!
            return null;
        }
    }
}