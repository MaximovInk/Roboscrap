using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MaximovInk
{
    public class RobotPartEditor : MonoBehaviour
    {
        public Transform partsViewer;
        public Button buttonPrefab;
        private SelectedPart _selectedPart;

        public Image head, body, la1, la2, la3, ra1, ra2, ra3, ll1, ll2, ll3, rl1, rl2, rl3;

        public Color onSelect = Color.white, onDeseletct = Color.black;

        private void Start()
        {
            CurrentType();
        }

        public void SelectPart(int part)
        {
            _selectedPart = (SelectedPart) part;
            var tp = CurrentType();
            var equalItems = GameManager.Instance.mainInventory.slots
                    .Where(n => n.item.item is RobotPartItem)
                    .Where(n =>
                        tp == 0 ? (n.item.item as RobotPartItem).part as Head_RP :
                        tp == 1 ? (n.item.item as RobotPartItem).part as Body_RP :
                        tp == 2 ? (n.item.item as RobotPartItem).part as ArmLeg_RP :
                        tp == 3 ? (n.item.item as RobotPartItem).part as Hand_RP :
                        tp == 4 ? (n.item.item as RobotPartItem).part as Leg_RP :
                        tp == 5 ? (n.item.item as RobotPartItem).part as Camera_RP :
                        (n.item.item as RobotPartItem).part).ToList()
                ;

            for (int i = 0; i < partsViewer.childCount; i++)
            {
                Destroy(partsViewer.transform.GetChild(i).gameObject);
            }

            var this_b = Instantiate(buttonPrefab, partsViewer);

            var this_part = GetPartFromPlayer();

            this_b.image.sprite = this_part.item.item.Icon;

            this_b.GetComponentInChildren<Text>().text =
                (int) (this_part.item.condition / this_part.item.item.MaxCondition * 100) + "%";

            this_b.interactable = false;

            for (int i = 0; i < equalItems.Count; i++)
            {
                var b = Instantiate(buttonPrefab, partsViewer);
                b.image.sprite = equalItems[i].item.item.Icon;
                var cond = (int) (equalItems[i].item.condition / equalItems[i].item.item.MaxCondition * 100);
                b.GetComponentInChildren<Text>().text = cond + "%";
                int j = i;
                b.onClick.AddListener(() =>
                {
                    ChangeTo(equalItems[j].item, (equalItems[j].item.item as RobotPartItem).part);
                });
            }
        }

        public void ChangeTo(ItemInstance iteminstance, RobotPart part)
        {
            Player.RobotM_P rp = null;
            switch (_selectedPart)
            {
                case SelectedPart.Head:
                    rp = GameManager.Instance.player.Head;
                    break;
                case SelectedPart.Body:
                    rp = GameManager.Instance.player.Body;
                    break;
                case SelectedPart.LA1:
                    rp = GameManager.Instance.player.la1;
                    break;
                case SelectedPart.LA2:
                    rp = GameManager.Instance.player.la2;
                    break;
                case SelectedPart.LA3:
                    rp = GameManager.Instance.player.la3;
                    break;
                case SelectedPart.RA1:
                    rp = GameManager.Instance.player.ra1;
                    break;
                case SelectedPart.RA2:
                    rp = GameManager.Instance.player.ra2;
                    break;
                case SelectedPart.RA3:
                    rp = GameManager.Instance.player.ra3;
                    break;
                case SelectedPart.LL1:
                    rp = GameManager.Instance.player.ll1;
                    break;
                case SelectedPart.LL2:
                    rp = GameManager.Instance.player.ll2;
                    break;
                case SelectedPart.LL3:
                    rp = GameManager.Instance.player.ll3;
                    break;
                case SelectedPart.RL1:
                    rp = GameManager.Instance.player.rl1;
                    break;
                case SelectedPart.RL2:
                    rp = GameManager.Instance.player.rl2;
                    break;
                case SelectedPart.RL3:
                    rp = GameManager.Instance.player.rl3;
                    break;
                case SelectedPart.CAM:
                    rp = GameManager.Instance.player.Cam;
                    break;
            }

            var item = rp.item;
            rp.item = iteminstance.Clone();
            GameManager.Instance.mainInventory.RemoveItem(iteminstance, 1);
            GameManager.Instance.mainInventory.AddItem(item.Clone());
            // var cond = rp.item.condition / rp.item.item.MaxCondition;
            /*if (_selectedPart == SelectedPart.RA3 || _selectedPart == SelectedPart.LA3)
                rp.sprite.spriteMesh = rp.part.meshes[0];
            else
                rp.sprite.spriteMesh = cond > 0.6f ? rp.part.meshes[0] :  cond > 0.3f ? rp.part.meshes[1] : rp.part.meshes[2];*/
            GameManager.Instance.player.UpdateSprites();
            SelectPart((int) _selectedPart);
        }

        private Player.RobotM_P GetPartFromPlayer()
        {
            switch (_selectedPart)
            {
                case SelectedPart.Head:
                    return GameManager.Instance.player.Head;
                case SelectedPart.Body:
                    return GameManager.Instance.player.Body;
                case SelectedPart.LA1:
                    return GameManager.Instance.player.la1;
                case SelectedPart.LA2:
                    return GameManager.Instance.player.la2;
                case SelectedPart.LA3:
                    return GameManager.Instance.player.la3;
                case SelectedPart.RA1:
                    return GameManager.Instance.player.ra1;
                case SelectedPart.RA2:
                    return GameManager.Instance.player.ra2;
                case SelectedPart.RA3:
                    return GameManager.Instance.player.ra3;
                case SelectedPart.LL1:
                    return GameManager.Instance.player.ll1;
                case SelectedPart.LL2:
                    return GameManager.Instance.player.ll2;
                case SelectedPart.LL3:
                    return GameManager.Instance.player.ll3;
                case SelectedPart.RL1:
                    return GameManager.Instance.player.rl1;
                case SelectedPart.RL2:
                    return GameManager.Instance.player.rl2;
                case SelectedPart.RL3:
                    return GameManager.Instance.player.rl3;
                case SelectedPart.CAM:
                    return GameManager.Instance.player.Cam;
                default:
                    return GameManager.Instance.player.Head;
            }
        }

        public int CurrentType()
        {
            head.color = onDeseletct;
            body.color = onDeseletct;
            la1.color = onDeseletct;
            la2.color = onDeseletct;
            la3.color = onDeseletct;
            ra1.color = onDeseletct;
            ra2.color = onDeseletct;
            ra3.color = onDeseletct;
            ll1.color = onDeseletct;
            ll2.color = onDeseletct;
            ll3.color = onDeseletct;
            rl1.color = onDeseletct;
            rl2.color = onDeseletct;
            rl3.color = onDeseletct;

            switch (_selectedPart)
            {
                case SelectedPart.Head:
                    head.color =onSelect;
                    return 0;
                case SelectedPart.Body:
                    body.color =onSelect;
                    return 1;
                case SelectedPart.LA1:
                    la1.color =onSelect;
                    return 2;
                case SelectedPart.LA2:
                    la2.color =onSelect;
                    return 2;
                case SelectedPart.LA3:
                    la3.color =onSelect;
                    return 3;
                case SelectedPart.RA1:
                    ra1.color =onSelect;
                    return 2;
                case SelectedPart.RA2:
                    ra2.color=onSelect;
                    return 2;
                case SelectedPart.RA3:
                    ra3.color=onSelect;
                    return 3;
                case SelectedPart.LL1:
                    ll1.color=onSelect;
                    return 2;
                case SelectedPart.LL2:
                    ll2.color=onSelect;
                    return 2;
                case SelectedPart.LL3:
                    ll3.color=onSelect;
                    return 4;
                case SelectedPart.RL1:
                    rl1.color =onSelect;
                    return 2;
                case SelectedPart.RL2:
                    rl2.color =onSelect;
                    return 2;
                case SelectedPart.RL3:
                    rl3.color =onSelect;
                    return 4;
                case SelectedPart.CAM:
                    return 5;
                default:
                    return -1;
            }
        }
    }

    public enum SelectedPart
    {
        None = -1,
        Head = 0,
        Body = 1,
        LA1 = 2,
        LA2 = 3,
        LA3 = 4,
        RA1 = 5,
        RA2 = 6,
        RA3 = 7,
        LL1 = 8,
        LL2 = 9,
        LL3 = 10,
        RL1 = 11,
        RL2 = 12,
        RL3 = 13,
        CAM = 14
    }
}